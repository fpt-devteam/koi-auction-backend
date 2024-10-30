const express = require("express");
const expressSession = require("express-session");
const cors = require("cors");
const cookieParser = require("cookie-parser");
const setupSwaggerDocs = require("./config/swagger");
const passport = require("passport");

require("dotenv").config();

const app = express();

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
app.use(
  expressSession({
    secret: "secret",
    resave: true,
    saveUninitialized: true,
  })
);
app.use(cookieParser());
app.use(passport.initialize());
app.use(passport.session());

app.use("/api", require("./router/user"));
setupSwaggerDocs(app);

const port = process.env.PORT || 3001;

app.listen(port, () => {
  console.log(`User service listening on 127.0.0.1:${port}`);
});
