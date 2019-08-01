﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MovieTheatre.DAL;
using MovieTheatre.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MovieTheatre.Controllers
{
    public class MovieController : Controller
    {
        private Context db = new Context();
        private Random rnd = new Random();

        // GET: Movie
        public ActionResult Index(string movieName)
        {
            var movies = from m in db.Movies
                        select m;

            if (!String.IsNullOrEmpty(movieName))
            {
                movies = movies.Where(s => s.Name.Contains(movieName));
            }
            var client = new WebClient();
            string httpString = "";
            foreach (var item in movies)
            {
                if (item.Poster == null || item.Description == null ||
                    item.Director == null || item.Year == null ||
                    item.Genre == null)
                {
                    httpString = "http://www.omdbapi.com/?t=" +
                                  item.Name +
                                  "&y=" + item.Year +
                                  "&apikey=4c2cc9b2";
                    var json = client.DownloadString(httpString);
                    var data = (JObject)JsonConvert.DeserializeObject(json);
                    if (data["Response"].Value<string>() == "True")
                    {
                        item.Poster = data["Poster"].Value<string>();
                        item.Description = data["Plot"].Value<string>();
                        item.Director = data["Director"].Value<string>();
                        item.Year = data["Year"].Value<string>();
                        item.Genre = data["Genre"].Value<string>();
                    }
                }
            }
            db.SaveChanges();

            // Genrate the Genres graph
            var genres = from m in db.Movies
                         group m by m.Genre into g
                         select new { Genre = g.Key, Amount = g.Count() };

            GenreListItem genreItem;
            GenreListItem[] genreList = new GenreListItem[genres.Count()];
            var index = 0;
            Color randomColor;
            foreach (var genre in genres)
            {
                genreItem = new GenreListItem();
                genreItem.label = genre.Genre;
                genreItem.value = genre.Amount;
                randomColor = new Color();
                randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                genreItem.color = randomColor.Name;
                genreList[index++] = genreItem;
            }
           ViewBag.genres = genreList;

            return View(movies.ToList());
        }

        // GET: Movie/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        public ActionResult CallWebService(Movie formMovie)
        {
            var client = new WebClient();
            Movie movie = new Movie();
            string httpString = "";
           /* httpString = "http://www.omdbapi.com/?t=" +
                                  name +
                                  //  "&y=" + item.Year +
                                  "&apikey=4c2cc9b2";
            var json = client.DownloadString(httpString);
            var data = (JObject)JsonConvert.DeserializeObject(json);
            if (data["Response"].Value<string>() == "True")
            {
                movie.Name = data["Title"].Value<string>();
                movie.Poster = data["Poster"].Value<string>();
                movie.Description = data["Plot"].Value<string>();
                movie.Director = data["Director"].Value<string>();
                movie.Year = data["Year"].Value<string>();
                movie.Genre = data["Genre"].Value<string>();
            }*/
            return RedirectToAction("Create", movie);
        }

        // GET: Movie/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Movie/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Genre,Description,Year,Director,Poster,Trailer")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                db.Movies.Add(movie);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(movie);
        }

        // GET: Movie/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Movie/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Genre,Description,Year,Director,Poster,Trailer")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                db.Entry(movie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Movie/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Movie movie = db.Movies.Find(id);
            db.Movies.Remove(movie);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
