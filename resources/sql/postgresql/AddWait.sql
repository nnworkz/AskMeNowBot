INSERT INTO waits (sender_id, recipient_id, type, is_response, link)
VALUES (@sender_id, @recipient_id, LOWER(@type), @is_response, @link)
ON CONFLICT (sender_id) DO UPDATE SET recipient_id = @recipient_id,
                                      type         = LOWER(@type),
                                      is_response  = @is_response,
                                      link         = @link
