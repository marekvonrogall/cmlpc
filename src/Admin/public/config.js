const BASE_PATH = "/admin";

fetch(`${BASE_PATH}/api/config`)
  .then((response) => {
    if (!response.ok) {
      throw new Error("Failed to fetch configuration.");
    }
    return response.json();
  })
  .then((data) => {
    const form = document.getElementById("config-form");

    Object.keys(data).forEach((groupName) => {
      const group = data[groupName];
      const groupContainer = document.createElement("div");

      const groupTitle = document.createElement("h2");
      groupTitle.textContent = groupName;
      groupContainer.appendChild(groupTitle);

      Object.keys(group).forEach((key) => {
        const item = group[key];

        const div = document.createElement("div");
        div.classList.add("form-group");

        const label = document.createElement("label");
        label.textContent = item.DisplayName;
        label.setAttribute("for", key);

        const input = document.createElement("input");
        input.value = item.Value;
        input.id = key;
        input.name = `${groupName}.${key}`;

        div.appendChild(label);
        div.appendChild(input);
        groupContainer.appendChild(div);
      });

      form.appendChild(groupContainer);
      form.appendChild(document.createElement("hr"));
    });
  })
  .catch((error) =>
    alert("Error loading configuration: " + error.message)
  );

function saveConfig() {
  const form = document.getElementById("config-form");
  const updatedConfig = {};

  Array.from(form.elements).forEach((element) => {
    if (element.type !== "button" && element.value.trim() !== "") {
      const value = element.value.trim();
      const [groupName, configKey] = element.name.split(".");
      if (!updatedConfig[groupName]) {
        updatedConfig[groupName] = {};
      }
      updatedConfig[groupName][configKey] = {
        value: isNaN(value) ? value : parseFloat(value),
      };
    }
  });

  fetch(`${BASE_PATH}/save-config`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(updatedConfig),
  })
    .then((response) => {
      if (response.ok) {
        alert("Configuration saved successfully.");
      } else {
        alert("Error saving configuration.");
      }
    })
    .catch((error) => {
      alert("Error saving configuration: " + error.message);
    });
}

function logout() {
  window.location.href = `${BASE_PATH}/logout`;
}