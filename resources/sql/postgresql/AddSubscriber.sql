INSERT INTO subscriptions (user_id, started_at, ends_at)
VALUES (@user_id, @started_at, @ends_at)
ON CONFLICT (user_id) DO UPDATE SET started_at = @started_at,
                                    ends_at    = @ends_at
