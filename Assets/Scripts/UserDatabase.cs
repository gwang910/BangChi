using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

[Serializable]
public class UserEntry
{
    public string id;
    public string salt;
    public string passwordHash;
}

[Serializable]
public class UserDbModel
{
    public List<UserEntry> users = new List<UserEntry>();
}

public static class UserDatabase
{
    private static string FileName => "users.json";
    private static string SrcPath => Path.Combine(Application.streamingAssetsPath, FileName);
    private static string DbPath  => Path.Combine(Application.persistentDataPath, FileName);

    private static UserDbModel _cache;

    /// 첫 실행 시 StreamingAssets의 기본 파일을 퍼시스턴트 경로로 복사
    public static void EnsureReady()
    {
        if (File.Exists(DbPath)) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android의 StreamingAssets는 파일 시스템이 아니므로 WWW/UnityWebRequest 필요
        string json = new WWW(SrcPath).text;
        File.WriteAllText(DbPath, json, Encoding.UTF8);
#else
        Directory.CreateDirectory(Application.persistentDataPath);
        File.Copy(SrcPath, DbPath, overwrite: true);
#endif
        _cache = null;
    }

    public static UserDbModel Load()
    {
        EnsureReady();
        if (_cache != null) return _cache;

        string json = File.ReadAllText(DbPath, Encoding.UTF8);
        _cache = JsonUtility.FromJson<UserDbModel>(json);
        if (_cache == null) _cache = new UserDbModel();
        return _cache;
    }

    private static void Save()
    {
        if (_cache == null) return;
        string json = JsonUtility.ToJson(_cache, prettyPrint: true);
        File.WriteAllText(DbPath, json, Encoding.UTF8);
    }

    public static bool Exists(string id)
    {
        var db = Load();
        return db.users.Exists(u => string.Equals(u.id, id, StringComparison.OrdinalIgnoreCase));
    }

    public static bool Register(string id, string plainPw)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrEmpty(plainPw)) return false;
        var db = Load();
        if (Exists(id)) return false;

        string salt = MakeSalt(16);
        string hash = HashPw(id, salt, plainPw);
        db.users.Add(new UserEntry { id = id, salt = salt, passwordHash = hash });
        Save();
        return true;
    }

    public static bool Validate(string id, string plainPw)
    {
        var db = Load();
        var user = db.users.Find(u => string.Equals(u.id, id, StringComparison.OrdinalIgnoreCase));
        if (user == null) return false;
        string hash = HashPw(id, user.salt, plainPw);
        return string.Equals(hash, user.passwordHash, StringComparison.Ordinal);
    }

    // ---------------- helpers ----------------
    private static string MakeSalt(int bytes)
    {
        var b = new byte[bytes];
        using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(b);
        return Convert.ToBase64String(b);
    }

    private static string HashPw(string id, string salt, string pw)
    {
        // 해시 입력 규칙은 반드시 고정: id:salt:pw
        string src = $"{id}:{salt}:{pw}";
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(src));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var x in bytes) sb.Append(x.ToString("X2")); // 대문자 Hex
        return sb.ToString();
    }
}
