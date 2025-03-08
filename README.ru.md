# 📨 AskMeNowBot <sup>[EN](README.md)</sup>

#### AskMeNowBot – бот для анонимных вопросов и ответов

### 🚀 Установка

>  🌱 Требуется **.NET 8.0**<br>
>  ✅ Работает на **Windows and Linux**<br>
>  ❗ Поддержка macOS **предполагается**, но **не тестировалась**

```bash
git clone https://github.com/nnworkz/askmenowbot.git
cd askmenowbot
```

### ⚙️ Настройка

-  Откройте `resources/config.json`
-  Выберите **базу данных** (MySQL / PostgreSQL)
-  Укажите **параметры подключения**
-  Сгенерируйте **ключ шифрования**
-  Введите **имя пользователя (username)** и **токен** бота

### 📦 Установка зависимостей

```bash
dotnet restore
```

### 🏁 Запуск

```bash
dotnet run
```
