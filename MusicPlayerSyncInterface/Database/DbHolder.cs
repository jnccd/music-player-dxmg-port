namespace MusicPlayerSyncInterface.Database;

public static class DbHolder
{
    static readonly object lockject = new();
    public static SongDbContext DbContext
    {
        get
        {
            lock (lockject)
            {
                return context;
            }
        }
        set
        {
            context = value;
        }
    }
    private static SongDbContext context = new();

    public static void SaveChanges()
    {
        lock (lockject)
        {
            context.SaveChanges();
        }
    }
}
