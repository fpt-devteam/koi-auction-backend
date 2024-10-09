const User = require('../model/user');
const argon2 = require('argon2');
const { sign } = require('jsonwebtoken');

const profile = async (req, res) => {
   const user = await User.findOne({ where: { UserId: req.user.userId } });
   res.status(200).json(user);
}

const login = async (req, res) => {
   const reqBody = req.body;
   const user = await User.findOne({ where: { Username: reqBody.username } });

   if (!user) {
      return res.status(401).json({ message: 'Username or Password is incorrect' });
   }

   if (!user.Active) {
      return res.status(401).json({ message: 'User is not available' });
   }

   const match = await argon2.verify(user.Password, reqBody.password).catch(err => {
      console.log(err);
   });

   if (match) {
      const accessToken = sign({ userId: user.UserId, userRoleId: user.UserRoleId }, process.env.JWT_SECRET);
      const signInExpire = reqBody['remember-me'] ? 1000 * 60 * 60 * 24 * 30 : 1000 * 60 * 60 * 24;
      res.cookie('access-token', accessToken, { httpOnly: true, secure: true, sameSite: 'none', maxAge: signInExpire });
      return res.status(200).json({ message: 'Login successful', user: user });
   }
   res.status(401).json({ message: 'Username or Password is incorrect' });
}

const register = async (req, res) => {
   const { username, password, firstname, lastname, phone, email } = req.body;
   const existUser = await User
      .findOne({ where: { [Op.or]: [{ Username: username }, { Email: email }] } })
      .catch(err => {
         console.log(err);
   });
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
      UserRoleId: 1,
      Balance: 0,
      CreatedAt: new Date(),
      UpdatedAt: new Date()
   }).catch(err => {
      console.log(err);
   });
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
   }, { where: { UserId: req.user.userId } }).catch(err => {
      console.log(err);
   });
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
   }, { where: { UserId: req.user.userId } }).catch(err => {
      console.log(err);
   });
   res.status(201).json({ message: "Password Updated"})
}

const logout = async (req, res) => {
   res.clearCookie('access-token');
   res.status(200).json({ message: 'Logout successful' });
}

const deleteAccount = async (req, res) => {
   await User.update({
      Active: false
   }, { where: { UserId: req.user.userId } }).catch(err => {
      console.log(err);
   });
   res.status(201).json({ message: "User Deleted"})
}

const getAllProfiles = async (req, res) => {
   const users = await User.findAll();
   res.status(200).json(users);
}

const getProfileById = async (req, res) => {
   const user = await User.findOne({ where: { UserId: req.params.id } });
   res.status(200).json(user);
}

module.exports = {
   profile,
   login,
   register,
   updateProfile,
   updatePassword,
   deleteAccount,
   logout,
   getAllProfiles,
   getProfileById
}