## About

This project calculates event costs based on predefined variables and user inputs.
It consists of the following services:

- cmlpc/nginx

  Acts as a gateway, routing traffic to the different services.
  Uses `nginx:alpine-slim` to route traffic to the different services.
  
- cmlpc/admin

  Provides the control panel for managing configuration settings.
  Uses `node:16-slim` to host a website providing functionality to modify the configuration file.

- cmlpc/cmlpc

  Price Calculator.
  Uses `mcr.microsoft.com/dotnet/sdk:9.0` to host an ASP.NET API, which returns event costs based on some user inputs and set variables in the config file.
  
- cmlpc/osrm

  Open Source Routing Machine.
  Uses `osrm/osrm-backend:latest` to calculate distances (by car) between two points.

## Usage

The ASP.NET API is accessible through the NGINX Gateway at the endpoint `calculateEventCosts`.

This endpoint requires several parameters in order to return a calculation.

- `plzRouteEnd` (required)

  This parameter specifies the postal code of the place where the event takes place.
  Only postal codes of Germany are valid inputs. Invalid inputs result in a 400 BadRequest error.

  This input is necessary to calculate the travel distance.
    
- `eventType` (required)

  This parameter specifies the event type of the event.
  Available event types are:

    - `Rollendes-Fotozimmer`
    - `Foto-Discokugel`
    - `Video-Kaleidoskop`

  Invalid inputs result in a 400 BadRequest error.
 
  This input is necessary to determine the cost per traveled kilometer.
  
- `eventDate` (required)

  This parameter specifies the date on which the event takes place.

  Only valid dates that can be formatted as a C# DateTime object using yyyy-MM-dd are accepted. Invalid inputs result in a 400 BadRequest error.

  This input is required to determine and apply discounts, if eligible.

- `additionalOptions` (optional)

  This parameter specifies additional options that can be booked for the event.

  Currently these additional options can be specified:

    - `XXL-Druck`

  AdditionalOptions need to be entered separated by a comma.

  `additionalOptions=Option1,Option2,Option3`
  
  This input is not required, as it is purely optional. Invalid inputs will be ignored without causing any errors.
  
**Example Usage:**

You can send requests using a http GET.

Example parameters:

  - plzRouteEnd: 30419
  - eventType: Rollendes-Fotozimmer
  - eventDate: 2025-02-27
  - additionalOptions: XXL-Druck

Example request:

`http://{hostAddress}:{NGINX_Port}/calculateEventCosts/?plzRouteEnd=30419&eventType=Rollendes-Fotozimmer&eventDate=2025-02-27&additionalOptions=XXL-Druck`

Response Body:

```json
[
    {
        "eventDuration": 5,
        "basePrice": 2500,
        "travelDistance": 674,
        "travelDuration": 10.11,
        "travelCosts": 1348,
        "hotelCosts": 250,
        "additionalOptions": [
            {
                "optionName": "XXL-Druck",
                "pricePerHour": 70,
                "priceTotal": 350
            }
        ],
        "totalCostWithoutDiscount": 4448,
        "totalCost": 3591.76,
        "applied": [
            {
                "name": "Offseason Sale",
                "periodStart": "2000-11-01T00:00:00",
                "periodEnd": "2001-03-31T00:00:00",
                "discountAmount": 667.2
            },
            {
                "name": "Spring Sale (Frühbucherrabatt)",
                "periodEnd": "2001-03-31T00:00:00",
                "discountAmount": 189.04
            }
        ],
        "vat": 682.4344
    },
    {
        "eventDuration": 8,
        "basePrice": 3300,
        "travelDistance": 674,
        "travelDuration": 10.11,
        "travelCosts": 1348,
        "hotelCosts": 250,
        "additionalOptions": [
            {
                "optionName": "XXL-Druck",
                "pricePerHour": 70,
                "priceTotal": 560
            }
        ],
        "totalCostWithoutDiscount": 5458,
        "totalCost": 4407.335,
        "applied": [
            {
                "name": "Offseason Sale",
                "periodStart": "2000-11-01T00:00:00",
                "periodEnd": "2001-03-31T00:00:00",
                "discountAmount": 818.7
            },
            {
                "name": "Spring Sale (Frühbucherrabatt)",
                "periodEnd": "2001-03-31T00:00:00",
                "discountAmount": 231.965
            }
        ],
        "vat": 837.39365
    }
]
```

The response body includes all the relevant information regarding the calculation including:

- eventDuration: Duration of the event in hours.
- basePrice: Base price of the event in €.
- travelDistance: Distance to the event location and back (start location specified in config file).
- travelDuration: Time needed to travel to the event duration and back.
- travelCosts: Travel costs in € based off the distance traveled and the price per traveled kilometer.
- hotelCosts: Hotel Costs in € (Either 0 or specified value, Based on a specific travel distance).
- additionalOptions: Lists booked additional option.
    - optionName: Name of the booked additional option.
    - pricePerHour: Price per hour in € of the additional option.
    - priceTotal: Price (total) in € of the additional option.
- totalCostWithoutDiscount: Total cost in € of the event without any discounts applied.
- totalCost: Total cost in € of the event with discounts applied.
- applied: Lists applied discounts.
    - name: Name of the applied discount.
    - periodStart: Start date of the applied discount (Only event based discounts).
    - periodEnd: End date of the applied discount (Both event and booking based discounts).
    - discountAmount: Applied discount amount in €.
- vat: Valued Added Tax in €.

The Control Panel is accessible through the NGINX Gateway at the endpoint `admin`.

`http://{hostAddress}:{NGINX_Port}/admin/`

It allows modification of the configuration file, which contains variables essential to the calculation process.
To access the configuration page, authentication with a username and password is required.
The default credentials are `admin`.
Refer to the VPS Prerequisites section for instructions on setting a custom username and password.

## VPS Prerequisites

This project is designed to run on a VPS using Docker.
The host machine must meet the following requirements to run the project:

- Minimum 6GB of memory
- Minimum 35GB of disk space

To specify a custom username and password for the Control Panel (`cmlpc/admin`), create a `.env` file in the same directory where your docker-compose is executed.
Store the .env file securely, as it contains sensitive information.

The environment file should include the following parameters:

- ADMIN_USERNAME

    Specifies a username to use for the control panel login.

- ADMIN_PASSWORD_HASH

    Specifies a password hash to use for the control panel. Do **not** enter your plain text password here.

    You can generate your own password hash using `bcrypt`.

    Example in Python:

    ```py
    import bcrypt
    password = "your_plain_text_password"
    salt = bcrypt.gensalt()
    hashed_password = bcrypt.hashpw(password.encode('utf-8'), salt)
    print(hashed_password.decode('utf-8'))
    input("Press any key to exit") 
    ```
    
Example `.env` file:

```env
ADMIN_USERNAME='YOUR_USERNAME'
ADMIN_PASSWORD_HASH='YOUR_PASSWORD_HASH'
```


