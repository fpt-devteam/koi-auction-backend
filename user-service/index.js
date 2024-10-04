const express = require('express');
const cookieParser = require('cookie-parser');
const setupSwaggerDocs = require('./config/swagger');

require('dotenv').config();

const app = express();

app.use(express.json());
app.use(cookieParser())

app.use('/api', require('./router/user'));
setupSwaggerDocs(app);

const port = process.env.PORT || 3001;

app.listen(port, () => {
   console.log(`User service listening on 127.0.0.1:${port}`);
})

