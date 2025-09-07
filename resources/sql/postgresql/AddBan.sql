INSERT INTO bans (sender_id, recipient_id, reason, banned_at)
VALUES (@sender_id, @recipient_id, @reason, @banned_at)
ON CONFLICT (sender_id, recipient_id) DO UPDATE SET reason    = @reason,
                                                    banned_at = @banned_at
