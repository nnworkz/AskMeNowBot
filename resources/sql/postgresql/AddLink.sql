INSERT INTO links (link, user_id, used, max_usages, is_deactivated, expires_at)
VALUES (@link, @user_id, @used, @max_usages, @is_deactivated, @expires_at)
ON CONFLICT (link) DO UPDATE SET link           = @link,
                                 user_id        = @user_id,
                                 used           = @used,
                                 max_usages     = @max_usages,
                                 is_deactivated = @is_deactivated,
                                 expires_at     = @expires_at
