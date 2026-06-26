using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using data.structs;

public static class SaveSystem
{
    // Ubah status ini menjadi false jika game sudah siap rilis (Production)
    private static readonly bool isDebugMode = true; 

    // Menggunakan ekstensi .json saat debug agar mudah dibuka langsung via Notepad
    static string savePath => Application.persistentDataPath + (isDebugMode ? "/save.json" : "/save.dat");

    static readonly byte[] aesKey = Encoding.UTF8.GetBytes("1234567890123456");
    static readonly byte[] hmacKey = Encoding.UTF8.GetBytes("chubby_sakee_123");

    public static void Save(GameState state)
    {
        string json = JsonUtility.ToJson(state, isDebugMode); // isDebugMode membuat format JSON berparagraf rapi (pretty print)

        if (isDebugMode)
        {
            // VERSI DEBUG: Tulis langsung teks JSON mentah tanpa pengaman apa pun
            File.WriteAllText(savePath, json);
            Debug.LogWarning($"[DEBUG SAVE] File berhasil disimpan (Tanpa Enkripsi) di: {savePath}");
        }
        else
        {
            // VERSI RELEASE: Menggunakan sistem enkripsi AES & validasi HMAC secara biner
            byte[] encrypted = Encrypt(json);
            byte[] hash = GenerateHMAC(encrypted);

            using (BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Create)))
            {
                writer.Write(hash.Length);
                writer.Write(hash);
                writer.Write(encrypted);
            }
            Debug.Log("File saved successfully with encryption.");
        }
    }

    public static void DeleteSaveFile()
    {
        if (File.Exists(savePath))
        {
            try
            {
                File.Delete(savePath);
                Debug.LogWarning($"File berhasil dihapus di: {savePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Gagal menghapus file: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("File tidak ditemukan, tidak ada yang dihapus.");
        }
    }

    public static GameState Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Save file tidak ditemukan.");
            return null;
        }

        if (isDebugMode)
        {
            // VERSI DEBUG: Baca langsung file teks JSON mentah
            string json = File.ReadAllText(savePath);
            Debug.LogWarning("[DEBUG LOAD] Berhasil memuat file save tanpa enkripsi.");
            return JsonUtility.FromJson<GameState>(json);
        }
        else
        {
            // VERSI RELEASE: Lakukan parsing biner, validasi integrity hash, lalu dekripsi
            using (BinaryReader reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
            {
                int hashLength = reader.ReadInt32();
                byte[] savedHash = reader.ReadBytes(hashLength);
                byte[] encrypted = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

                byte[] newHash = GenerateHMAC(encrypted);

                if (!CompareHashes(savedHash, newHash))
                {
                    Debug.LogError("Save file tampered!");
                    return null;
                }

                string json = Decrypt(encrypted);
                return JsonUtility.FromJson<GameState>(json);
            }
        }
    }

    #region SecuritySystem (Hanya Berjalan Saat isDebugMode = false)
    static byte[] Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = aesKey;
            aes.GenerateIV();
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return ms.ToArray();
            }
        }
    }

    static string Decrypt(byte[] cipherData)
    {
        using (Aes aes = Aes.Create())
        {
           byte[] iv = new byte[16];
            System.Array.Copy(cipherData, iv, iv.Length);
            aes.Key = aesKey;
            aes.IV = iv;

            using (MemoryStream ms = new MemoryStream(cipherData, 16, cipherData.Length - 16))
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

    static byte[] GenerateHMAC(byte[] data)
    {
        using (HMACSHA256 hmac = new HMACSHA256(hmacKey))
        {
            return hmac.ComputeHash(data);
        }
    }

    static bool CompareHashes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }
    #endregion
}