const express = require("express");
const cors = require("cors");
const cookieParser = require("cookie-parser");
const setupSwaggerDocs = require("./config/swagger");

require("dotenv").config();

const app = express();

const corsOptions = {
  origin: ["http://localhost:5173"],
  credentials: true,
};
app.use(cors(corsOptions));
app.use(express.json());
app.use(cookieParser());

app.use("/api", require("./router/user"));
setupSwaggerDocs(app);

const port = process.env.PORT || 3001;

app.listen(port, () => {
  console.log(`User service listening on 127.0.0.1:${port}`);
});
