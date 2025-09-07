CREATE TABLE IF NOT EXISTS subscriptions
(
    user_id    BIGINT PRIMARY KEY,
    started_at TIMESTAMP(0) NOT NULL,
    ends_at    TIMESTAMP(0) NOT NULL
)
