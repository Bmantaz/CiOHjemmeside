-- =============================================================================
-- Nulstil ALLE adgangskoder på siden
-- =============================================================================
--
-- Hvad scriptet gør:
--   1. Sætter mustresetpassword = true for ALLE brugere (inkl. Admins).
--   2. Overskriver passwordhash med en tilfældig, ubrugelig værdi, så ingen
--      kan logge ind med deres gamle adgangskode.
--
-- Efter kørsel:
--   - Ingen kan logge ind med eksisterende adgangskoder.
--   - Når en bruger forsøger at logge ind, fejler login, og Login.razor
--     sender dem videre til /reset-password/{id}, hvor de sætter en ny.
--   - Brugeren skal kun kende sit BRUGERNAVN for at komme videre.
--
-- VIGTIGT: Tag en backup af users-tabellen før du kører dette.
--   CREATE TABLE users_backup_20250101 AS SELECT * FROM users;
--
-- Kræver: pgcrypto (til gen_random_bytes). Kør evt.:
--   CREATE EXTENSION IF NOT EXISTS pgcrypto;
-- =============================================================================

BEGIN;

-- Sikkerhedstjek: vis hvor mange brugere der rammes, før ændringen.
-- Kør denne linje alene først, hvis du vil se antallet:
--   SELECT COUNT(*) FROM users;

UPDATE users
SET
	-- En ugyldig bcrypt-lignende streng. BCrypt.Verify vil altid fejle på den,
	-- så den gamle adgangskode kan ikke længere bruges.
	passwordhash = '$2a$11$' || encode(gen_random_bytes(24), 'hex'),
	mustresetpassword = true;

-- Kontrollér resultatet før du committer.
SELECT id, username, role, mustresetpassword
FROM users
ORDER BY username;

COMMIT;

-- Fortryd i stedet med: ROLLBACK;
