# ServiceHttpConfig
A Windows Service with some configuration via HTTP

## What is it? 

This is a (very) basic `dotnet` core application that has the ability to run as a console or Windows Service application.

After connecting to local RabbitMq instance it does the following things:

* Declare a `topic` exchange (`ServiceApp.Exchange`)
* Declare a durable non-exclusive queue (`ServiceApp.Exchange.Queue`)
* Bind the queue to the exchange
* Listen to the exchange

## That's all?

No! Additionally using [App.Metrics](https://www.app-metrics.io/) it sets up a counter to count the number of messages processed. These metrics are then exposed on the `/metrics` endpoint. (When running locally this is `http://localhost:5000/metrics`.)

## Anything else?

Yup. Using `Mvc` a web page is exposed `/` (locally `http://localhost:5000`) and this tells you if the service is paused or not. The page also has a button on it that allows you to pause or unpause the service.