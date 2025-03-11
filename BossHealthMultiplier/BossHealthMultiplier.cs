using System;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

[ApiVersion(2, 1)]
public class BossHealthMultiplier : TerrariaPlugin
{
    public override string Name => "BossHealthMultiplier";
    public override string Author => "YourName";
    public override string Description => "Mengalikan HP boss sesuai multiplier yang diatur di konfigurasi.";
    public override Version Version => new Version(1, 0, 0);

    private const string ConfigPath = "BossHealthMultiplierConfig.json";
    private BossConfig config;

    public BossHealthMultiplier(Main game) : base(game)
    {
        Order = 0;
    }

    public override void Initialize()
    {
        LoadConfig();
        ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
    }

    private void OnNpcSpawn(NpcSpawnEventArgs args)
    {
        // Jika NPC adalah boss, ubah HP-nya
        if (args.Npc.boss)
        {
            // Ambil multiplier dari config
            float multiplier = config.HealthMultiplier;

            // Jika multiplier tidak 1 (artinya ada perubahan), ubah HP boss
            if (multiplier != 1.0f)
            {
                args.Npc.lifeMax = (int)(args.Npc.lifeMax * multiplier);
                args.Npc.life = args.Npc.lifeMax;
            }
        }
    }

    private void LoadConfig()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                // Jika file tidak ada, buat file config dengan multiplier default 1 (tidak ada perubahan)
                config = new BossConfig { HealthMultiplier = 1.0f };
                SaveConfig();
            }
            else
            {
                string json = File.ReadAllText(ConfigPath);
                config = JsonConvert.DeserializeObject<BossConfig>(json);
            }
        }
        catch (Exception ex)
        {
            TShock.Log.Error("Gagal memuat konfigurasi BossHealthMultiplier: " + ex.Message);
            // Jika terjadi error, gunakan multiplier default
            config = new BossConfig { HealthMultiplier = 1.0f };
        }
    }

    private void SaveConfig()
    {
        try
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            TShock.Log.Error("Gagal menyimpan konfigurasi BossHealthMultiplier: " + ex.Message);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);
        }
        base.Dispose(disposing);
    }
}

public class BossConfig
{
    // Atur multiplier HP boss, misalnya 2.5 untuk menggandakan HP boss sebanyak 2.5 kali lipat.
    public float HealthMultiplier { get; set; }
}
