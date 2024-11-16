const express = require('express');
const sendEmail = require('./utils/sendEmail');
const app = express();
const cors = require('cors');
const User = require('./models/user');

require("dotenv").config();

const corsOptions = {
   origin: [
      "http://localhost:5173",
      "https://b4e3-2401-d800-5aec-76ef-c58c-ff24-9016-45ef.ngrok-free.app",
      "http://localhost:3000",
   ],
   credentials: true,
};

app.use(cors(corsOptions));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

app.post("/api/send-email", async (req, res) => {
   const { userId, subject, text } = req.body;
   const { Email, Subject, Text } = req.body;
   console.log(req.body);
   if (Email && Subject && Text) {
      await sendEmail(Email, Subject, Text);
      return res.status(200).json({ message: "Email sent successfully "});
   }
   else if (userId && subject && text) {
      const user = await User.findOne({ where: { UserId: userId } });
      const email = user.Email;
      await sendEmail(email, subject, text);
      return res.status(200).json({ message: "Email sent successfully "});
   }
   return res.status(400).send("Missing required fields");
});

const port = process.env.PORT || 3005;

app.listen(port, () => {
   console.log(`Mail service listening on 127.0.0.1:${port}`);
});