SELECT EXISTS (SELECT 1 FROM subscriptions WHERE user_id = @user_id)
