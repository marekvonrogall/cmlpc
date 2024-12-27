const express = require("express");
const fs = require("fs");
const bcrypt = require("bcryptjs");
const path = require("path");
const dotenv = require("dotenv");

dotenv.config();

const app = express() ;
const port = 8083;

const ADMIN_USERNAME = process.env.ADMIN_USERNAME || "admin";
const ADMIN_PASSWORD_HASH = process.env.ADMIN_PASSWORD_HASH || "admin";

const BASE_PATH = "/admin"; // Base path for Nginx proxy
const fullPath = (path) => BASE_PATH + path;

app.use(BASE_PATH, express.static("public"));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

let isAuthenticated = false;

app.get(fullPath("/"), (req, res) => {
  res.sendFile(path.join(__dirname, "login.html"));
});

app.post(fullPath("/login"), (req, res) => {
  const { username, password } = req.body;

  if (username === ADMIN_USERNAME && bcrypt.compareSync(password, ADMIN_PASSWORD_HASH)) {
    isAuthenticated = true;
    return res.redirect(fullPath("/config"));
  } else {
    res.redirect(fullPath("/?loginFailed=true"));
  }
});

const authenticate = (req, res, next) => {
  if (!isAuthenticated) {
    return res.redirect(fullPath("/")); // Redirect to login if not authenticated
  }
  next();
};

app.get(fullPath("/config"), authenticate, (req, res) => {
  fs.readFile(path.join(__dirname, "config", "config.json"), "utf8", (err, data) => {
    if (err) {
      return res.status(500).send("Error loading config file");
    }

    res.send(data);
  });
});

app.post(fullPath("/save-config"), authenticate, (req, res) => {
  const updatedConfig = req.body;

  fs.writeFile(path.join(__dirname, "config", "config.json"), JSON.stringify(updatedConfig, null, 4), (err) => {
    if (err) {
      return res.status(500).send("Error saving config file");
    }

    res.send("Configuration saved successfully");
  });
});

app.listen(port, () => {
  console.log(`Server running on http://localhost:${port}`);
});
