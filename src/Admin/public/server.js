const express = require("express");
const fs = require("fs");
const bcrypt = require("bcryptjs");
const path = require("path");
const dotenv = require("dotenv");

dotenv.config();

const app = express();
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

  if (
    username === ADMIN_USERNAME &&
    bcrypt.compareSync(password, ADMIN_PASSWORD_HASH)
  ) {
    isAuthenticated = true;
    return res.redirect(fullPath("/config"));
  } else {
    res.redirect(fullPath("/?loginFailed=true"));
  }
});

app.get(fullPath("/logout"), (req, res) => {
  isAuthenticated = false;
  res.redirect(fullPath("/"));
});

const authenticate = (req, res, next) => {
  if (!isAuthenticated) {
    return res.redirect(fullPath("/")); // Redirect to login if not authenticated
  }
  next();
};

app.get(fullPath("/config"), authenticate, (req, res) => {
  res.sendFile(path.join(__dirname, "config.html"));
});

app.get(fullPath("/api/config"), authenticate, (req, res) => {
  fs.readFile(
    path.join(__dirname, "config", "config.json"),
    "utf8",
    (err, data) => {
      if (err) {
        return res.status(500).json({ error: "Error loading config file" });
      }
      try {
        const config = JSON.parse(data);
        res.json(config);
      } catch (parseErr) {
        res.status(500).json({ error: "Invalid JSON format in config file" });
      }
    }
  );
});

app.post(fullPath("/save-config"), authenticate, (req, res) => {
  const updatedConfig = req.body;

  fs.readFile(path.join(__dirname, "config", "config.json"), "utf8", (err, data) => {
    if (err) {
      return res.status(500).json({ error: "Error reading config file" });
    }

    try {
      let currentConfig = JSON.parse(data);

      Object.keys(updatedConfig).forEach((groupName) => {
        const group = updatedConfig[groupName];
        Object.keys(group).forEach((key) => {
          if (currentConfig[groupName] && currentConfig[groupName][key]) {
            currentConfig[groupName][key].value = group[key].value;
          }
        });
      });

      fs.writeFile(path.join(__dirname, "config", "config.json"), JSON.stringify(currentConfig, null, 2), (err) => {
        if (err) {
          return res.status(500).json({ error: "Error saving config file" });
        }
        res.json({ success: true });
      });

    } catch (parseErr) {
      return res.status(500).json({ error: "Invalid JSON format in config file" });
    }
  });
});


app.listen(port, () => {
  console.log(`Server running on http://localhost:${port}`);
});
