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
        // Ambil index NPC dari event args
        int npcIndex = args.NpcId;

        // Pastikan index valid (kadang bisa -1 atau melebihi Main.maxNPCs)
        if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
            return;

        // Ambil NPC dari array Main.npc
        NPC npc = Main.npc[npcIndex];

        // Jika NPC adalah boss, ubah HP-nya
        if (npc.boss)
        {
            float multiplier = config.HealthMultiplier;

            // Jika multiplier != 1, artinya ada perubahan HP
            if (multiplier != 1.0f)
            {
                npc.lifeMax = (int)(npc.lifeMax * multiplier);
                npc.life = npc.lifeMax;
            }
        }
    }

    private void LoadConfig()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                // Buat file config default dengan multiplier 1.0
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
            // Gunakan multiplier default jika gagal
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
    // Nilai multiplier HP boss. Contoh: 2.5 berarti boss punya HP 2.5x lipat.
    public float HealthMultiplier { get; set; }
}
