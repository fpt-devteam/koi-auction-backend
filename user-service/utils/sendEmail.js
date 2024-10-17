const nodemailer = require("nodemailer");

const sendEmail = async (email, subject, text) => {
   try {
      const transporter = nodemailer.createTransport({
         host: 'smtp.gmail.com',
         port: 587,
         service: 'gmail',
         auth: {
            user: process.env.USER,
            pass: process.env.PASS,
         },
      });

      await transporter.sendMail({
         from: process.env.USER,
         to: email,
         subject: subject,
         text: text,
      });
   }
   catch (error) {
      console.log(error);
   }
};

module.exports = sendEmail;