using Dapper;

namespace CiOHjemmeside.Data.Services
{
    /// <summary>
    /// Runs all schema-creation/migration DDL once at application startup.
    /// This replaces the previous pattern of running CREATE TABLE IF NOT EXISTS /
    /// ALTER TABLE ADD COLUMN IF NOT EXISTS on every request inside individual
    /// service methods, which added unnecessary DDL overhead to every query.
    /// </summary>
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IDbConnectionFactory connectionFactory)
        {
            using var connection = await connectionFactory.CreateConnectionAsync();

            const string sql = @"
                CREATE TABLE IF NOT EXISTS sales (
                    id SERIAL PRIMARY KEY,
                    soldat TIMESTAMPTZ NOT NULL,
                    soldbyuserid INT NOT NULL,
                    totalamount NUMERIC(10,2) NOT NULL
                );
                CREATE TABLE IF NOT EXISTS saleitems (
                    id SERIAL PRIMARY KEY,
                    saleid INT NOT NULL REFERENCES sales(id) ON DELETE CASCADE,
                    productgroupname TEXT NOT NULL,
                    variantname TEXT NOT NULL,
                    quantity INT NOT NULL,
                    unitprice NUMERIC(10,2) NOT NULL,
                    lineamount NUMERIC(10,2) NOT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_sales_soldat ON sales (soldat);
                CREATE INDEX IF NOT EXISTS idx_saleitems_saleid ON saleitems (saleid);

                CREATE TABLE IF NOT EXISTS epkassets (
                    id SERIAL PRIMARY KEY,
                    category TEXT NOT NULL,
                    title TEXT NOT NULL,
                    filename TEXT NOT NULL,
                    contenttype TEXT NOT NULL,
                    filesize BIGINT NOT NULL,
                    content BYTEA NOT NULL,
                    thumbnail BYTEA,
                    sortorder INT NOT NULL DEFAULT 0,
                    uploadedat TIMESTAMPTZ NOT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_epkassets_category ON epkassets (category, sortorder);

                ALTER TABLE concerts ADD COLUMN IF NOT EXISTS otherbands TEXT;
                ALTER TABLE concerts ADD COLUMN IF NOT EXISTS facebookeventlink TEXT;

                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'calendarevents_eventtype_check') THEN
                        ALTER TABLE calendarevents DROP CONSTRAINT calendarevents_eventtype_check;
                    END IF;
                    ALTER TABLE calendarevents
                        ADD CONSTRAINT calendarevents_eventtype_check
                        CHECK (eventtype IN ('Gig','Practice','Discord','Other','Unavailable'));
                END $$;
            ";

            await connection.ExecuteAsync(sql);
        }
    }
}
