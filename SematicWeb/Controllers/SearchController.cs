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

        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://localhost:3030/ds/sparql"));
        bool searchPoGlumcu = false;
        bool searchPoFilmu = true;



        // GET: Search
        public ActionResult Index()
        {

            /*
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
            */

            // SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");


            //Make a SELECT query against the Endpoint
            SparqlResultSet results = endpoint.QueryWithResultSet("PREFIX lmdb: <http://data.linkedmdb.org/resource/movie/> SELECT DISTINCT ?actorName WHERE { ?kb lmdb:actor_name 'Kevin Bacon' . ?movie lmdb:actor ?kb . ?movie lmdb:actor ?actor . ?actor lmdb:actor_name ?actorName . FILTER(?kb != ?actor)}  ORDER BY ASC(?actorName)");
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
            //Statistika
            SparqlResultSet ukupanBrojFilmova = endpoint.QueryWithResultSet("PREFIX movie: <http://data.linkedmdb.org/resource/movie/> SELECT (COUNT(*) AS ?ukupnoFilmova) WHERE { ?movie a movie:film}");
            homeData.Stats.MoviesCount = Int32.Parse(((VDS.RDF.BaseLiteralNode)(ukupanBrojFilmova.Results[0]["ukupnoFilmova"])).Value); ;

            SparqlResultSet ukupanBrojTrojki = endpoint.QueryWithResultSet("SELECT(COUNT(*) AS ?ukupnoTrojki ) WHERE { ?s ?p ?o }");
            homeData.Stats.TriplesCount = Int32.Parse(((VDS.RDF.BaseLiteralNode)(ukupanBrojTrojki.Results[0]["ukupnoTrojki"])).Value);


            SparqlResultSet ukupanBrojGlumaca = endpoint.QueryWithResultSet("PREFIX glumci: <http://data.linkedmdb.org/resource/movie/> SELECT (COUNT(*) AS ?ukupnoGlumaca) WHERE { ?actor a glumci:actor}");
            homeData.Stats.ActorsCount = Int32.Parse(((VDS.RDF.BaseLiteralNode)(ukupanBrojGlumaca.Results[0]["ukupnoGlumaca"])).Value);

            SparqlResultSet ukupnoDramskihFilmova = endpoint.QueryWithResultSet("PREFIX movie: <http://data.linkedmdb.org/resource/movie/> SELECT (COUNT(*) AS ?brojDramskihFilmova) WHERE { ?movie movie:genre ?genre . ?genre movie:film_genre_name ?gname . VALUES ?gname {\"Drama\"} }");
            homeData.Stats.DramaMoviesCount = Int32.Parse(((VDS.RDF.BaseLiteralNode)(ukupnoDramskihFilmova.Results[0]["brojDramskihFilmova"])).Value); ;

            return View(homeData);
        }

        //tu pretrazi neki pojam i dobije rezultat
        public ActionResult Search(string Query)
        {
            Search search = new Search();
            search.results = new List<SearchResult>();

            if (searchPoFilmu)
            {
                SparqlResultSet pretragaPoNaslovuFilma = endpoint.QueryWithResultSet("PREFIX movie: <http://data.linkedmdb.org/resource/movie/> PREFIX dc: <http://purl.org/dc/terms/>SELECT ?title  WHERE {?movie dc:title ?title .FILTER regex(?title, '" + Query + "', 'i')}ORDER BY ASC(?title)");
                foreach (SparqlResult result in pretragaPoNaslovuFilma)
                {
                    Movie m = new Movie();
                    m.name = ((VDS.RDF.BaseLiteralNode)(result[0])).Value;
                    search.results.Add(m);
                }
            }

            if (searchPoGlumcu)
            {
                SparqlResultSet pretragaPoImenuGlumca = endpoint.QueryWithResultSet("PREFIX lmdb: <http://data.linkedmdb.org/resource/movie/> SELECT DISTINCT ?actorName WHERE { ?movie lmdb:actor ?personURI . ?personURI lmdb:actor_name ?actorName FILTER regex(?actorName, '" + Query + "', 'i')}ORDER BY ASC(?actorName)");
                foreach (SparqlResult result in pretragaPoImenuGlumca)
                {
                    Actor a = new Actor();
                    a.name = ((VDS.RDF.BaseLiteralNode)(result[0])).Value;
                    search.results.Add(a);
                }
            }

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

        public ActionResult SearchDetails(string query)
        {
            //ako je autor prikazi detalje autora 

            //inace prikazi detalje filma

            if (searchPoFilmu)
            {

                SparqlResultSet detaljiFilma = endpoint.QueryWithResultSet("PREFIX movie: <http://data.linkedmdb.org/resource/movie/> PREFIX dc: <http://purl.org/dc/terms/> SELECT distinct ?title ?genreName ?dirName ?editorName ?date WHERE {?movie dc:title ?title . ?movie movie:genre ?genre. ?genre movie:film_genre_name ?genreName. ?movie movie:director ?director. ?director movie:director_name ?dirName. ?movie movie:editor ?editor. ?editor movie:editor_name ?editorName. ?movie dc:date ?date  FILTER REGEX (?title, '" + query + "', 'i') }ORDER BY ASC(?title) ");

                Movie m = new Movie();
                if (detaljiFilma.Count > 0)
                {
                    SparqlResult result = detaljiFilma[0];             
                    m.name = ((VDS.RDF.BaseLiteralNode)(result[0])).Value;
                    m.scenarist = ((VDS.RDF.BaseLiteralNode)(result[2])).Value;
                    m.redatelj = ((VDS.RDF.BaseLiteralNode)(result[3])).Value;
                    m.datumNastanka = ((VDS.RDF.BaseLiteralNode)(result[4])).Value;
                }
                return View("~/Views/Search/Movie.cshtml", m);
            }

            if (searchPoGlumcu)
            {
                Actor actor = new Models.Actor();

                return View("~/Views/Search/Actor.cshtml", actor);
            }

            return View("~/Views/Search/NoResult.cshtml");
        }

    }
}