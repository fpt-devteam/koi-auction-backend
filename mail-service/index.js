const express = require('express');
const sendEmail = require('./utils/sendEmail');
const app = express();
const cors = require('cors');

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
   const { Email, Subject, Text } = req.body;
   if (!Email || !Subject || !Text) {
      return res.status(400).send("Missing required fields");
   }
   await sendEmail(Email, Subject, Text);
   res.send("Email sent successfully");
});

const port = process.env.PORT || 3005;

app.listen(port, () => {
   console.log(`Mail service listening on 127.0.0.1:${port}`);
});