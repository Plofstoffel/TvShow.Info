# TvShow.Info
WebApi designed to scrape and display info of TVShows.

[![.NET](https://github.com/Plofstoffel/TvShow.Info/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Plofstoffel/TvShow.Info/actions/workflows/workflows/dotnet.yml)
[![Coverage Status](https://coveralls.io/repos/github/Plofstoffel/TvShow.Info/badge.svg?branch=main)](https://coveralls.io/github/Plofstoffel/TvShow.Info?branch=main)
[![GitHub license](https://img.shields.io/github/license/Plofstoffel/TvShow.Info)](https://github.com/Plofstoffel/TvShow.Info/blob/main/LICENSE)


## General

The main function of this project is to scrape data from the [TvMaze Api](https://www.tvmaze.com/api)
* Scraper was written using a background service.
* Api is a .net 6 Web Api.

## How to use the scraper

* One or more scrapers can be run in Docker
* The "TvShowsUrl" setting can be used to set the api to srape. (The scraper surrently only supports the TvMaze Api)
* The Scraper makes use of a Cercuit braker to eliminate failures or rate limmiting on the scraped Api
    * The "RequestErrorTimeout" setting is used to set the timeout in seconds after a http error code is recieved.
    * The "RequestErrorRetries" settting will set the amount of retries before the scraper throws an exception.
* The "TvShowScrapeLimit" setting is used to set the amount of items is scraped ine a run.
* The "StaleTvShowUpdateFrequency" is used to check the TvMaze Api for updated information based on the following values [day, week, month] 

## How to use the API

* There is a maximum nuber of shows you can retrieve per page. This can be set in the config using the "MaxEntriesPerPage" setting. 

# TvShows.Info.Api
## Version: 1.0

### /api/TvShow/GetTvShows

#### GET
##### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| pageSize | query |  | No | integer |
| pageNumber | query |  | No | integer |

##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 400 | Bad Request |
| 500 | Server Error |

#### Example Response

```json
[
  {
    "id": 0,
    "name": "Invader Zim",
    "cast": [
      {
        "id": 0,
        "name": "Zim",
        "birthday": "2022-11-07T00:51:36.340Z",
        "tvShows": [
          "Invader Zim",
          "Some Other Show"
        ]
      },
      {
        "id": 0,
        "name": "GIR",
        "birthday": "2022-11-07T00:51:36.340Z",
        "tvShows": [
          "Invader Zim"
        ]
      }
    ]
  }
]
```