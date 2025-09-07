CREATE TABLE IF NOT EXISTS filters
(
    user_id     BIGINT PRIMARY KEY,
    spam        BOOLEAN NOT NULL,
    terrorism   BOOLEAN NOT NULL,
    drugs       BOOLEAN NOT NULL,
    violence    BOOLEAN NOT NULL,
    pornography BOOLEAN NOT NULL
)
