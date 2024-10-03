const express = require('express');
const cookieParser = require('cookie-parser');
const argon2 = require('argon2');
const User = require('./models/user');
const { sign } = require('jsonwebtoken');

require('dotenv').config();

const app = express();

app.use(express.json());
app.use(cookieParser())


app.get('/profile', async (req, res) => {
   const userID = req.headers["uid"];

   const user = await User.findByPk(userID);

   if (!userID) {
      console.log("Unauthorized");
      return res.status(401).json({ message: 'Unauthorized' });
   }

   if (!user) {
      return res.status(404).json({ message: 'User not found' });
   }
   res.status(200).json(user);
})

app.post('/login', async (req, res) => {
   const { username, password } = req.body;
   const user = await User.findOne({ where: { Username: username } });

   if (!user) {
      return res.status(401).json({ message: 'Username or Password is incorrect' });
   }

   const match = await argon2.verify(user.Password, password).catch(err => {
      console.log(err);
   });

   if (match) {
      const accessToken = sign({ userID: user.UserId, username: username }, process.env.JWT_SECRET);
      res.cookie('access-token', accessToken, { httpOnly: true, secure: true, sameSite: 'none' });
      return res.json({ message: 'Login successful' });
   }
   res.status(401).json({ message: 'Username or Password is incorrect' });
})

app.post('/register', async (req, res) => {
   const { username, password, firstname, lastname, phone, email } = req.body;
   const existUser = await User.findOne({ where: { Username: username } });
   if (existUser) {
      return res.status(400).json({ message: 'User already exists' });
   }
   const hashedPassword = await argon2.hash(password, 10);
   await User.create({
      Username: username,
      Password: hashedPassword,
      FirstName: firstname,
      LastName: lastname,
      Phone: phone,
      Email: email
   })
   res.status(201).json({ message: "User Created"})
})

const port = process.env.PORT || 3001;

app.listen(port, () => {
   console.log(`User service listening on 127.0.0.1:${port}`);
})

