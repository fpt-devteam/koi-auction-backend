const User = require('../model/user');
const argon2 = require('argon2');
const { sign } = require('jsonwebtoken');

const profile = async (req, res) => {
   const user = await User.findOne({ where: { UserId: req.user.userId } });
   res.status(200).json(user);
}

const login = async (req, res) => {
   const { username, password } = req.body;
   const user = await User.findOne({ where: { Username: username } });

   if (!user) {
      return res.status(401).json({ message: 'Username or Password is incorrect' });
   }

   if (!user.Active) {
      return res.status(401).json({ message: 'User is not available' });
   }

   const match = await argon2.verify(user.Password, password).catch(err => {
      console.log(err);
   });

   if (match) {
      const accessToken = sign({ userId: user.UserId, username: username }, process.env.JWT_SECRET);
      res.cookie('access-token', accessToken, { httpOnly: true, secure: true, sameSite: 'none' });
      return res.json({ message: 'Login successful' });
   }
   res.status(401).json({ message: 'Username or Password is incorrect' });
}

const register = async (req, res) => {
   const { username, password, firstname, lastname, phone, email, roleId } = req.body;
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
      Email: email,
      Active: true,
      UserRoleId: roleId,
      Balance: 0,
      CreatedAt: new Date(),
      UpdatedAt: new Date()
   })
   res.status(201).json({ message: "User Created"})
}

const updateProfile = async (req, res) => {
   const { username, firstname, lastname, phone, email, roleId } = req.body;
   await User.update({
      Username: username,
      FirstName: firstname,
      LastName: lastname,
      Phone: phone,
      Email: email,
      UserRoleId: roleId,
      CreatedAt: new Date(),
      UpdatedAt: new Date()
   }, { where: { UserId: req.user.userId } })
   res.status(201).json({ message: "User Updated"})
}

const updatePassword = async (req, res) => {
   const { password } = req.body;
   if (!password) {
      return res.status(400).json({ message: 'Password is required' });
   }
   const hashedPassword = await argon2.hash(password, 10);
   await User.update({
      Password: hashedPassword
   }, { where: { UserId: req.user.userId } })
   res.status(201).json({ message: "Password Updated"})
}

const logout = async (req, res) => {
   res.clearCookie('access-token');
   res.status(200).json({ message: 'Logout successful' });
}

const deleteAccount = async (req, res) => {
   await User.update({
      Active: false
   }, { where: { UserId: req.user.userId } })
   res.status(201).json({ message: "User Deleted"})
}

module.exports = {
   profile,
   login,
   register,
   updateProfile,
   updatePassword,
   deleteAccount,
   logout
}