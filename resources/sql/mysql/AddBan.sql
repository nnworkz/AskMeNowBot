INSERT INTO bans (sender_id, recipient_id, reason, banned_at)
VALUES (@sender_id, @recipient_id, @reason, @banned_at)
ON DUPLICATE KEY UPDATE reason    = @reason,
                        banned_at = @banned_at
