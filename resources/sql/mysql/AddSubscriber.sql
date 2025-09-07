INSERT INTO subscriptions (user_id, started_at, ends_at)
VALUES (@user_id, @started_at, @ends_at)
ON DUPLICATE KEY UPDATE started_at = @started_at,
                        ends_at    = @ends_at
