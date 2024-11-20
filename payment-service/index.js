const express = require('express'); // npm install express
const cors = require("cors"); // npm install cors

const app = express();
const port = 3004;

const corsOptions = {
   origin: [
      'http://localhost:5173',
      "https://koi-auction-bice.vercel.app",
   ],
   credentials: true,
};
app.use(cors(corsOptions));
app.use(express.json());

app.use('/api', require('./router'));

app.listen(port, () => {
   console.log(`Payment service listening on 127.0.0.1:${port}`);
});