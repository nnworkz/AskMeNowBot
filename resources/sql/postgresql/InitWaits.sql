CREATE TABLE IF NOT EXISTS waits
(
    sender_id    BIGINT PRIMARY KEY,
    recipient_id BIGINT,
    type         VARCHAR(16) NOT NULL,
    is_response  BOOLEAN,
    link         VARCHAR(12)
)
