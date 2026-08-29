using System.Text.Json;

namespace NexaTweaks.Core.Booster;

public sealed class GameProfileStore
{
    private readonly string _path;

    public GameProfileStore(string? rootOverride = null)
    {
        var dir = rootOverride ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NexaTweaks");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "game-profiles.json");
    }

    public List<GameProfile> Load()
    {
        var defaults = DefaultProfiles();
        if (!File.Exists(_path)) return defaults;
        try
        {
            var saved = JsonSerializer.Deserialize<List<GameProfile>>(File.ReadAllText(_path)) ?? new List<GameProfile>();
            return MergeWithDefaults(saved, defaults);
        }
        catch
        {
            return defaults;
        }
    }

    public void Save(IEnumerable<GameProfile> profiles) =>
        File.WriteAllText(_path, JsonSerializer.Serialize(profiles.ToList(), new JsonSerializerOptions { WriteIndented = true }));

    private static List<GameProfile> MergeWithDefaults(IEnumerable<GameProfile> saved, IEnumerable<GameProfile> defaults) =>
        saved.Concat(defaults)
            .GroupBy(profile => profile.ProcessName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(profile => profile.ProcessName, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static List<GameProfile> DefaultProfiles() => new()
    {
        new("Fortnite", "FortniteClient-Win64-Shipping"),
        new("GTA V", "GTA5"),
        new("FiveM", "FiveM_b2372_GTAProcess"),
        new("Counter-Strike 2", "cs2"),
        new("Minecraft", "javaw"),
        new("Valorant", "VALORANT-Win64-Shipping"),
        new("League of Legends", "LeagueClient"),
        new("Warzone", "cod"),
        new("Apex Legends", "r5apex"),
        new("Roblox", "RobloxPlayerBeta"),
        new("God of War", "GoW"),
        new("God of War Ragnarok", "GoWRagnarok"),
        new("MTA: San Andreas", "Multi Theft Auto"),
        new("MTA: San Andreas", "gta_sa"),
        new("Euro Truck Simulator 1", "eurotrucks"),
        new("Euro Truck Simulator 2", "ets2"),
        new("Rainbow Six Siege", "RainbowSix"),
        new("Cult Of the Lamb", "CultOfTheLamb"),
        new("Ultrakill", "ULTRAKILL"),
        new("BloodStrike", "BloodStrike"),
        new("Arena Breakout", "ArenaBreakout"),
        new("Resident Evil 4 Remake", "re4"),
        new("Resident Evil 2 Remake", "re2"),
        new("Resident Evil Village", "re8"),
        new("Free Fire / BlueStacks", "HD-Player"),
        new("Battlefield 2042", "BF2042"),
        new("Battlefield 4", "bf4"),
        new("The Last of Us Part I", "tlou-i"),
        new("The Last of Us Part II", "tlou-ii"),
        new("PUBG", "tslgame"),
        new("Rocket League", "RocketLeague"),
        new("Cyberpunk 2077", "Cyberpunk2077"),
        new("Terraria", "Terraria"),
        new("Red Dead Redemption 2", "RDR2"),
        new("Battlefield 6", "BF6"),
        new("Choo Choo Charles", "Charles"),
        new("Hell Let Loose", "HLL"),
        new("Farming Simulator 22", "FarmingSimulator2022"),
        new("Farming Simulator 25", "FarmingSimulator2025"),
        new("Hollow Knight", "hollow_knight"),
        new("Genshin Impact", "GenshinImpact"),
        new("Point Blank", "PointBlank"),
        new("My Summer Car", "mysummercar"),
        new("DayZ", "DayZ"),
        new("Street Fighter 6", "StreetFighter6"),
        new("Rust", "RustClient"),
        new("Chivalry 2", "Chivalry2-Win64-Shipping"),
        new("Subnautica", "Subnautica"),
        new("Left 4 Dead", "left4dead"),
        new("Left 4 Dead 2", "left4dead2"),
        new("Marvel Rivals", "MarvelRivals"),
        new("Warface", "Warface"),
        new("Deadlock", "Deadlock"),
        new("Cuphead", "Cuphead"),
        new("Escape From Tarkov", "EscapeFromTarkov"),
        new("Death Stranding", "ds"),
        new("Death Stranding 2", "DeathStranding2"),
        new("Poppy Playtime", "Poppy_Playtime"),
        new("Poppy Playtime Multiplayer", "Playtime_Multiplayer"),
        new("Project Playtime", "ProjectPlaytime"),
        new("Poppy Playtime Chapter 4", "PoppyPlaytimeChapter4"),
        new("Poppy Playtime Chapter 5", "PoppyPlaytimeChapter5"),
        new("PES 2017", "PES2017"),
        new("PES 2018", "PES2018"),
        new("PES 2019", "PES2019"),
        new("PES 2020", "PES2020"),
        new("eFootball", "eFootball"),
        new("Dead by Daylight", "DeadByDaylight-Win64-Shipping"),
        new("EA SPORTS FC 26", "FC26"),
        new("Final Fantasy XIV", "ffxiv_dx11"),
        new("Ghost of Tsushima", "GhostOfTsushima"),
        new("Skyrim Special Edition", "SkyrimSE"),
        new("Days Gone", "DaysGone"),
        new("Palworld", "Palworld-Win64-Shipping"),
        new("CrossFire", "crossfire"),
        new("Warframe", "Warframe.x64"),
        new("The Isle", "TheIsleClient-Win64-Shipping"),
        new("SnowRunner", "SnowRunner"),
        new("REMATCH", "REMATCH"),
        new("Meccha Chameleon", "MecchaChameleon"),
    };
}
