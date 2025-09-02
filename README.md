<a id="readme-top"></a>

<!-- PROJECT LOGO -->
<br />
<div align="center">

<h3 align="center">Hacker News Project</h3>

  <p align="center">
    
   </p>
</div>


<!-- ABOUT THE PROJECT -->
## About The Project

This project is part of a coding challenge.  The challenge...

Using the <a href="https://github.com/HackerNews/API">Hacker News API</a>, create a solution that allows users to view the newest stories from the feed.
 
Your solution must be developed using an Angular front-end app that calls a C# .NET Core back-end restful API. 
 
The front-end UI should consist of: 
* A list of the newest stories
* Each list item should include the title and a link to the news article. Watch out for stories that do not have hyperlinks!
* A search feature that allows users to find stories
* A paging mechanism. While we love reading, 200 stories on the page is a bit much.
* Automated tests for your code
 
The back-end API should consist of: 
* Usage of dependency injection, it's built-in so why not?
* Caching of the newest stories
* Automated tests for your code
 
While we would love seeing an Angular only solution to this problem, we really need to see your CSharpness.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Built With
* [![NET-Core-9][NET-Core-9]][NET-Core-9-url]
* [![Angular][Angular.io]][Angular-url]

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started
### Prerequisites
* Visual Studio 2022
* npm
* Web Browser

### Installation

1. Clone the repo
   ```sh
   git clone https://github.com/andersontechnologygroup/hackernews.git
   ```

   This will clone both the back-end and front-end code.

2. Install NPM packages

    Go into the HackerNewsClient folder and install NPM packages

   ```sh
   cd HackerNewsClient
   npm install
   ```
3. Run Unit Tests for C#

    From the root folder, run tests

   ```sh
   dotnet test
   ```
4. Run Stryker mutation tests for C#

    From the root folder, run Stryker.  For convience, run the batch file.

   ```sh
   .\str
   ```

    Stryker will open a report in your web browser.   The report may seem to be blank while the tests are being run in the background.   Once all tests are complete, the report will update itself.

5. Run Unit Tests for Angular

    Go into the HackerNewsClient folder and run unit tests


   ```sh
   cd HackerNewsClient
   npm test
   ```
6. Run C# API Project

    * Open the project in Visual Studio.  
    * src/HackerNews.Api should be set as the default project.  If not, set it as the startup.   
    * Click Debug > Start Debugging

7. Run Angular Project

    * Open Powershell or Command Prompt and navigate to the HackerNewsClient folder 
    * Start the Angular Project with the npm command
    ```sh
    npm start
    ```
8. Open a browser
    * Open a browser and navigate to 
    ```html
    http://localhost:4200
    ```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- USAGE EXAMPLES -->
## Usage

This Hacker News viewer is, currently, very limited.  

On the C# back-end, the code provides an API with get and search functionality.   The back-end, when requested, will call to the Hacker News API and retrieve the most current new stories.   These stories are then cached into the back-end for 5 minutes.   Currently, there is no incremental caching.   Once the cache expires, the entire set of news stories are reloaded.  Only a maximum of 200 stories are gathered from Hacker News at a time.  Only stories with valid URLs are gathered, thus leading to a fewer than 200 stories possibilty being gathered.  

On the Agnular front-end, the code calls the backend API to gather a complete list of stories.  This list is paginated to 10 stories per page.  At the top and bottom of the current page, Previous and Next buttons are provided to navigate the pages.  The stories title, user, score, and time are displayed as part of the "storry card".   Clicking on the card will open the URL in a new tab/window.  Pagination is done completely on the client side.  

Two search fields are provided: Title and User.  A title search will do a partial match of the entered keyword.  This match is case insensitive.  A user search will do an exact match of the entered user against the stories user.   This match is case insensitive.  Searching is done on the back-end by calling into the API.

## API Configuration

An appsettings file (JSON format) is set up on the back-end side to provide a few different configurations that can be changed without re-compiling code.   The settings are as follows.

* CacheKey
    * Default: "HackerNewsStoryCacheKey"
    * This is the internal name of the cache that the back-end uses.  
* CacheTimeoutInMinutes
    * Default: 5
    * The length of time, in minutes, that the cache will retain information. 
* NewStoriesJSONPath
    * Default: "newstories.json"
    * The relative path of the Hacker News API to get the new stories.
* ItemJSONPath
    * Default: "item/{storyId}.json"
    * The relative path of the Hacker News API to get the details of a single item/story.
* NumberOfStoriesToPull
    * Default: 200
    * The maximum number of stories that the back-end will attempt to gather on each call to the Hacker News API.

## Potential expansion

Several potential expansions have already been identified (and some implemented in part).

* Incremental caching of stories.
    * As mentioned, the current functionality dumps all data in the cache when it refreshes and pulls the newest stories.  Adding incremental refreshing of the cache would allow for the existing stories to be retained when a refresh occurs and only the newest stories would need to be added to the cache.  The list of stories would, therefore, continue to expand.  This model would require several additional considerations to build out completely.  

* Authentification/Authorization
    * With this very limited functionality, there is no need for authentification.  However, with very little expansion (adding a database to store stories longer term, etc), the need for administrative functionality increases.   This will require authentification.  As part of this, JWT tokens would be implemented and used when calling some API on the back-end.   This has, technically, been built in to the back-end already, although the front-end is not making use of it.

* Long term storage of stories.
    * Adding a database will allow for the long term storage of stories.  This model would require several additional considerations to build out completely.

* Additional searches
    * Currently, only Title and User are searchable.   Potential fields that could be quickly added are Score and Time.   

* Additional filtering considerations
    * Several field are available that could be used to further filter out stories.  Deleted and Dead immediately come to mind.  

* Additional presentation of story data.
    * Hacker News provides much more fiunctionality that simply getting a likst of stories.   Each story has a score, potential polls, parents, kids, etc.  Little to none of this information is presented currently.   

* Client side sorting
    * Currently, there is no sorting provided either on the back-end or front-end.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTACT -->
## Contact

Jason Anderson - jbanderson2009@gmail.com

<p align="right">(<a href="#readme-top">back to top</a>)</p>

[NET-Core-9]: https://img.shields.io/badge/Core%209-9780e5?style=for-the-badge&logo=dotnet
[NET-Core-9-url]: https://dotnet.microsoft.com/en-us

[Angular.io]: https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white
[Angular-url]: https://angular.io/
