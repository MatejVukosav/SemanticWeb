using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SematicWeb.Models;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace SematicWeb.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index()
        {
            //Create a Parameterized String
            SparqlParameterizedString queryString = new SparqlParameterizedString();

            //Add a namespace declaration
            queryString.Namespaces.AddNamespace("lmdb", new Uri("http://data.linkedmdb.org/resource/movie/"));

            queryString.CommandText = "SELECT DISTINCT ? actorName WHERE { ?kb lmdb:actor_name \"Kevin Bacon\".? movie lmdb:actor? kb . ?movie lmdb:actor? actor . ?actor lmdb:actor_name? actorName .FILTER(?kb != ? actor)}  ORDER BY ASC(?actorName)";


            //Inject a Value for the parameter
            queryString.SetUri("value", new Uri("http://localhost:3030/LMDB/query"));

            //When we call ToString() we get the full command text with namespaces appended as PREFIX
            //declarations and any parameters replaced with their declared values
            Console.WriteLine(queryString.ToString());



           // SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");

            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://localhost:3030/ds/sparql"));


            //Make a SELECT query against the Endpoint
            SparqlResultSet results = endpoint.QueryWithResultSet("PREFIX lmdb: <http://data.linkedmdb.org/resource/movie/> SELECT DISTINCT ? actorName WHERE { ?kb lmdb:actor_name \"Kevin Bacon\".? movie lmdb:actor? kb . ?movie lmdb:actor? actor . ?actor lmdb:actor_name? actorName .FILTER(?kb != ? actor)}  ORDER BY ASC(?actorName)");
            foreach (SparqlResult result in results)
            {
                Console.WriteLine(result.ToString());
            }


            Home homeData = new Home();

            homeData.randomMovies = new List<Movie>();

            Movie m1 = new Movie();
            m1.name = "Bilo jednom u americi";
            m1.url = "http://nesto.com";
            m1.redatelj = "marjan";
            m1.scenarist = "perica";

            Actor actor = new Actor();
            actor.name = "milivoj";
            m1.actors = new List<Models.Actor>();
            m1.actors.Add(actor);

            homeData.randomMovies.Add(m1);

            homeData.Stats = new Stats();
            homeData.Stats.ActorsCount = 1;
            homeData.Stats.MoviesCount = 2;
            homeData.Stats.TriplesCount = 6;
            homeData.Stats.WritersCount = 4;

            return View(homeData);
        }

        public ActionResult Search(string Query)
        {
            Search search = new Search();

            search.results = new List<SearchResult>();

            Movie m = new Movie();
            m.name = "ovo je film";

            Actor a = new Actor();
            a.name = "ovo je autor";

            search.results.Add(m);
            search.results.Add(a);

            //tu pretrazi neki pojam i dobije rezultat
            return View(search);
        }

        public ActionResult Actor(Actor actor)
        {
            return View();
        }

        public ActionResult Movie(string movie)
        {
            return View();
        }

        public ActionResult Stats()
        {
            return View();
        }

        public ActionResult SearchDetails()
        {
            //ako je autor prikazi detalje autora 

            //inace prikazi detalje filma

            return View();
        }
    }
}