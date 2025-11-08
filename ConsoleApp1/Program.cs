using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello World!");
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            //var filePath = System.IO.Directory.GetFiles(currentDirectory, "*.csv").First();
            var filePath = "C:\\Users\\Никита\\RiderProjects\\ConsoleApp1\\ConsoleApp1\\tmdb_5000_credits.csv";

            IReadOnlyList<MovieCredit> movieCredits = null;
            try
            {
                var parser = new MovieCreditsParser(filePath);
                movieCredits = parser.Parse(); // Тип переменной теперь IReadOnlyList<MovieCredit>
            }
            catch (Exception exc)
            {
                Console.WriteLine("Не удалось распарсить csv");
                Environment.Exit(1);
            }

            var top10Actors = movieCredits
                .SelectMany(movie => movie.Cast) // Объединяем всех актеров из всех фильмов в одну последовательность
                .GroupBy(castMember => castMember.Name) // Группируем по имени актера
                .Select(group => new
                {
                    ActorName = group.Key,
                    MovieCount = group.Count() // Считаем количество фильмов для каждого
                })
                .OrderByDescending(actor => actor.MovieCount) // Сортируем по убыванию количества фильмов
                .Take(10); // Берем первые 10

            Console.WriteLine(string.Join(Environment.NewLine,
                top10Actors.Select(a => $"{a.ActorName} - {a.MovieCount}")));

            Console.WriteLine("\nНайти все фильмы, снятые режиссером \"Steven Spielberg\".\n");
            var spielbergFilms = from m in movieCredits
                where m.Crew.Any(crew => crew.Name == "Steven Spielberg" && crew.Job == "Director")
                select m;
            Console.WriteLine("Фильмы:");
            foreach (var film in spielbergFilms)
            {
                Console.WriteLine(film.Title);
            }

            Console.WriteLine("\nПолучить список всех персонажей, которых сыграл актер \"Tom Hanks\".\n");

            var tomHanksCharacters = from m in movieCredits
                where m.Cast.Any(member => member.Name == "Tom Hanks")
                select m;
            Console.WriteLine("Персонажи, которых сыграл Tom Hanks:");
            foreach (var film in tomHanksCharacters)
            {
                Console.WriteLine(film.Cast.First(member => member.Name == "Tom Hanks").Character);
            }

            Console.WriteLine("\nНайти 5 фильмов с самым большим количеством актеров в составе.\n");

            var top5Movies = movieCredits
                .Select(movie => new
                {
                    MovieName = movie.Title,
                    ActorsCount = movie.Cast.Count
                }).OrderByDescending(m => m.ActorsCount)
                .Take(5);
            foreach (var movie in top5Movies)
            {
                Console.WriteLine("Имя фильма {0}. Кол-во актеров {1}", movie.MovieName, movie.ActorsCount);
            }

            Console.WriteLine(
                "\n Найти топ-10 самых востребованных актеров (по количеству фильмов). - сделано в примере лектора\n");

            Console.WriteLine("\nПолучить список всех уникальных департаментов (department) съемочной группы.\n");

            var departmentFilms = movieCredits
                .SelectMany(movie => movie.Crew)
                .Select(m => m.Department)
                .Distinct()
                .OrderBy(department => department);

            Console.WriteLine("Уникальные департаменты съемочной группы:");
            foreach (var department in departmentFilms)
            {
                Console.WriteLine(department);
            }

            Console.WriteLine("\nНайти все фильмы, где \"Hans Zimmer\" был композитором (Original Music Composer).\n");

            var hansZimmerMovies = movieCredits
                .Where(movie => movie.Crew.Any(crew =>
                    crew.Name == "Hans Zimmer" &&
                    crew.Job == "Original Music Composer"))
                .Select(movie => movie.Title);


            Console.WriteLine("Фильмы с музыкой Hans Zimmer:");
            foreach (var movieTitle in hansZimmerMovies)
            {
                Console.WriteLine(movieTitle);
            }

            Console.WriteLine("\nСоздать словарь, где ключ — ID фильма, а значение — имя режиссера.\n");

            var movieCreatorMap = movieCredits.ToDictionary(m => m.MovieId, m => m.Crew
                .Where(member => member.Job == "Director")
                .Select(crew => crew.Name));

            Console.WriteLine("\nНайти фильмы, где в актерском составе есть и \"Brad Pitt\", и \"George Clooney\".\n");

            var filmsWithBradAndGeorge = movieCredits.Where(movie => movie.Cast.Any(crew => crew.Name == "Brad Pitt")
                                                                     && movie.Cast.Any(crew =>
                                                                         crew.Name == "George Clooney"))
                .Select(movie => movie.Title);

            foreach (var m in filmsWithBradAndGeorge)
            {
                Console.WriteLine(m);
            }

            Console.Write("\nПосчитать, сколько всего человек работает в департаменте \"Camera\" по всем фильмам.\n");

            var cameraCount = movieCredits.Where(movie => movie.Crew.Any(crew => crew.Department == "Camera"))
                .Select(movie => movie.Crew.Count).ToList().Sum();
            Console.WriteLine(cameraCount);

            Console.WriteLine(
                "\nНайти всех людей, которые в фильме \"Titanic\" были одновременно и в съемочной группе, и в списке актеров.\n");

            var titanicMovie = movieCredits.FirstOrDefault(movie => movie.Title == "Titanic");

            if (titanicMovie != null)
            {
                var peopleInCastAndCrew = titanicMovie.Cast
                    .Where(actor => titanicMovie.Crew.Any(crew => crew.Name == actor.Name))
                    .Select(actor => actor.Name)
                    .ToList();

                foreach (var person in peopleInCastAndCrew)
                {
                    Console.WriteLine(person);
                }
            }

            Console.WriteLine(
                "\nНайти \"внутренний круг\" режиссера: Для режиссера \"Quentin Tarantino\" найти топ-5 членов съемочной группы (не актеров), которые работали с ним над наибольшим количеством фильмов.\n");

            var tarantinoInnerCircle = movieCredits
                .Where(movie => movie.Crew.Any(crew =>
                    crew.Name == "Quentin Tarantino" && crew.Job == "Director"))
                .SelectMany(movie => movie.Crew //Берм из всех фильмов только персонал
                    .Where(crew => crew.Name != "Quentin Tarantino")) //которые не Тарантино
                .GroupBy(crew => crew.Name)
                .Select(group => new
                {
                    Name = group.Key,
                    FilmCount = group.Count()
                })
                .OrderByDescending(person => person.FilmCount)
                .Take(5)
                .ToList();

            Console.WriteLine("Топ-5 членов съемочной группы Quentin Tarantino:");
            foreach (var person in tarantinoInnerCircle)
            {
                Console.WriteLine($"{person.Name} - {person.FilmCount} фильмов");
            }

            Console.WriteLine(
                "\nОпределить экранные \"дуэты\": Найти 10 пар актеров, которые чаще всего снимались вместе в одних и тех же фильмах.\n");

            var actorDuos = movieCredits
                .Where(movie => movie.Cast.Count >= 2)
                .SelectMany(movie =>
                {
                    var actors = movie.Cast.Select(a => a.Name).ToList();
                    var pairs = new List<(string, string)>();

                    for (int i = 0; i < actors.Count; i++)
                    {
                        for (int j = i + 1; j < actors.Count; j++)
                        {
                            var actor1 = actors[i];
                            var actor2 = actors[j];
                            // Сортируем имена для уникальности пары
                            if (string.Compare(actor1, actor2) < 0)
                                pairs.Add((actor1, actor2));
                            else
                                pairs.Add((actor2, actor1));
                        }
                    }

                    return pairs;
                })
                .GroupBy(pair => pair)
                .Select(group => new
                {
                    Actor1 = group.Key.Item1,
                    Actor2 = group.Key.Item2,
                    MovieCount = group.Count()
                })
                .OrderByDescending(duo => duo.MovieCount)
                .Take(10)
                .ToList();

            Console.WriteLine("Топ-10 самых частых экранных дуэтов:");
            foreach (var duo in actorDuos)
            {
                Console.WriteLine($"{duo.Actor1} и {duo.Actor2}: {duo.MovieCount} фильмов вместе");
            }

            Console.WriteLine(
                "\nВычислить 'индекс разнообразия' для карьеры: Найти 5 членов съемочной группы, которые поработали в наибольшем количестве различных департаментов за свою карьеру.\n");

            var diversityLeaders = movieCredits
                .SelectMany(movie => movie.Crew)
                .GroupBy(crew => crew.Name)
                .Select(group => new
                {
                    Name = group.Key,
                    DepartmentCount = group.Select(c => c.Department).Distinct().Count(),
                    JobCount = group.Select(c => c.Job).Distinct().Count(),
                    MovieCount = group.Count()
                })
                .Where(person => person.DepartmentCount >= 2) // минимум 2 департамента
                .OrderByDescending(person => person.DepartmentCount)
                .ThenByDescending(person => person.JobCount)
                .Take(5)
                .ToList();

            Console.WriteLine("Топ-5 по индексу разнообразия:");
            foreach (var person in diversityLeaders)
            {
                Console.WriteLine(
                    $"{person.Name}: {person.DepartmentCount} департаментов, {person.JobCount} должностей, {person.MovieCount} фильмов");
            }

            Console.WriteLine(
                "\nНайти 'творческие трио': Найти фильмы, где один и тот же человек выполнял роли режиссера (Director), сценариста (Writer) и продюсера (Producer).\n");

            var creativeTrios = movieCredits
                .Select(movie => new
                {
                    Movie = movie.Title,
                    Directors = movie.Crew.Where(c => c.Job == "Director").Select(c => c.Name).ToList(),
                    Writers = movie.Crew.Where(c => c.Job == "Writer").Select(c => c.Name).ToList(),
                    Producers = movie.Crew.Where(c => c.Job == "Producer").Select(c => c.Name).ToList(),
                    AllCrew = movie.Crew
                })
                .Where(movie =>
                    movie.Directors.Any() &&
                    movie.Writers.Any() &&
                    movie.Producers.Any())
                .Select(movie => new
                {
                    Movie = movie.Movie,
                    TripleThreatPeople = movie.Directors
                        .Intersect(movie.Writers)
                        .Intersect(movie.Producers)
                        .ToList()
                })
                .Where(movie => movie.TripleThreatPeople.Any())
                .ToList();

// Выводим результат
            if (creativeTrios.Any())
            {
                Console.WriteLine("Фильмы с 'творческими трио' (один человек - режиссер, сценарист и продюсер):");
                foreach (var trio in creativeTrios)
                {
                    Console.WriteLine($"\n{trio.Movie}");
                    foreach (var person in trio.TripleThreatPeople)
                    {
                        Console.WriteLine($" {person} - Director, Writer, Producer");
                    }
                }
            }
            else
            {
                Console.WriteLine("Фильмы с творческими трио не найдены.");
            }

            Console.WriteLine(
                "\nДва шага до Кевина Бейкона: Найти всех актеров, которые снимались в одном фильме с актером, который, в свою очередь, снимался в одном фильме с 'Kevin Bacon'.\n");

            var kevinBaconMovies = movieCredits
                .Where(movie => movie.Cast.Any(actor => actor.Name == "Kevin Bacon"))
                .Select(movie => movie.MovieId)
                .ToList();

            Console.WriteLine($"Фильмов с Kevin Bacon: {kevinBaconMovies.Count}");

            var firstStepActors = movieCredits
                .Where(movie => kevinBaconMovies.Contains(movie.MovieId))
                .SelectMany(movie => movie.Cast)
                .Where(actor => actor.Name != "Kevin Bacon")
                .Select(actor => actor.Name)
                .Distinct()
                .ToList();

            Console.WriteLine($"Актеров на первом шаге (снимались с Kevin Bacon): {firstStepActors.Count}");

            var secondStepMovies = movieCredits
                .Where(movie => movie.Cast.Any(actor => firstStepActors.Contains(actor.Name)))
                .Select(movie => movie.MovieId)
                .Distinct()
                .ToList();

            var secondStepActors = movieCredits
                .Where(movie => secondStepMovies.Contains(movie.MovieId))
                .SelectMany(movie => movie.Cast)
                .Where(actor => !firstStepActors.Contains(actor.Name) && actor.Name != "Kevin Bacon")
                .Select(actor => actor.Name)
                .Distinct()
                .ToList();

            Console.WriteLine($"Актеров на втором шаге от Kevin Bacon: {secondStepActors.Count}");

            Console.WriteLine("\nАктеры на расстоянии двух шагов от Kevin Bacon:");
            foreach (var actor in secondStepActors.Take(20)) // Покажем первые 20
            {
                Console.WriteLine($"• {actor}");
            }

            if (secondStepActors.Count > 20)
            {
                Console.WriteLine($"... и еще {secondStepActors.Count - 20} актеров");
            }

            Console.WriteLine(
                "\nПроанализировать 'командную работу': Сгруппировать фильмы по режиссеру и для каждого из них найти средний размер как актерского состава (Cast), так и съемочной группы (Crew).\n");

            var directorTeamAnalysis = movieCredits
                .Select(movie => new
                {
                    Movie = movie,
                    Directors = movie.Crew
                        .Where(crew => crew.Job == "Director")
                        .Select(crew => crew.Name)
                        .ToList()
                })
                .SelectMany(movieWithDirectors => movieWithDirectors.Directors
                    .Select(director => new
                    {
                        Director = director,
                        Movie = movieWithDirectors.Movie
                    }))
                .GroupBy(item => item.Director)
                .Select(group => new
                {
                    Director = group.Key,
                    MovieCount = group.Count(),
                    AvgCastSize = group.Average(m => m.Movie.Cast.Count),
                    AvgCrewSize = group.Average(m => m.Movie.Crew.Count),
                    TotalCastSize = group.Sum(m => m.Movie.Cast.Count),
                    TotalCrewSize = group.Sum(m => m.Movie.Crew.Count),
                    Movies = group.Select(m => m.Movie.Title).ToList()
                })
                .OrderByDescending(d => d.MovieCount)
                .ThenByDescending(d => d.AvgCastSize)
                .ToList();

            Console.WriteLine("Анализ командной работы режиссеров:");
            foreach (var director in directorTeamAnalysis.Take(15))
            {
                Console.WriteLine($"\n {director.Director}");
                Console.WriteLine($"   Фильмов: {director.MovieCount}");
                Console.WriteLine($"   Средний размер актерского состава: {director.AvgCastSize:F1}");
                Console.WriteLine($"   Средний размер съемочной группы: {director.AvgCrewSize:F1}");
                Console.WriteLine($"   Примеры фильмов: {string.Join(", ", director.Movies.Take(3))}");
            }

            Console.WriteLine($"\n Всего режиссеров: {directorTeamAnalysis.Count}");
            Console.WriteLine(
                $"Средний размер актерского состава по всем режиссерам: {directorTeamAnalysis.Average(d => d.AvgCastSize):F1}");
            Console.WriteLine(
                $"Средний размер съемочной группы по всем режиссерам: {directorTeamAnalysis.Average(d => d.AvgCrewSize):F1}");

            

            Console.WriteLine("\nНайти пересечение 'элитных клубов': Найти людей, которые работали и с режиссером 'Martin Scorsese', и с режиссером 'Christopher Nolan'.\n");

            

            var departmentInfluence = movieCredits
                .SelectMany(movie => movie.Crew.Select(crew => new { crew.Department, movie.Cast.Count }))
                .GroupBy(x => x.Department)
                .Select(g => new
                {
                    Department = g.Key,
                    AvgCastSize = g.Average(x => x.Count),
                    MovieCount = g.Count()
                })
                .OrderByDescending(x => x.AvgCastSize)
                .ToList();

            Console.WriteLine("Ранжирование департаментов по среднему размеру актерского состава:");
            foreach (var dept in departmentInfluence)
            {
                Console.WriteLine($"{dept.Department}: {dept.AvgCastSize:F1} актеров (в {dept.MovieCount} фильмах)");
            }
            Console.WriteLine("\nПроанализировать 'архетипы' персонажей: Для актера 'Johnny Depp' сгруппировать его роли по первому слову в имени персонажа и посчитать частоту каждого такого 'архетипа'.\n");

            var deppRoles = movieCredits
                .SelectMany(movie => movie.Cast)
                .Where(actor => actor.Name == "Johnny Depp" && !string.IsNullOrEmpty(actor.Character))
                .Select(actor => actor.Character.Trim())
                .ToList();

            var roleArchetypes = deppRoles
                .Select(role =>
                {
                    var firstSpace = role.IndexOf(' ');
                    var firstWord = firstSpace > 0 ? role.Substring(0, firstSpace) : role;
        
                    firstWord = firstWord.Trim(',', '.', '!', '?', '"', '\'', '(', ')', '[', ']');
        
                    return firstWord;
                })
                .Where(word => !string.IsNullOrEmpty(word))
                .GroupBy(word => word)
                .Select(g => new
                {
                    Archetype = g.Key,
                    Frequency = g.Count(),
                    Examples = deppRoles.Where(role => role.StartsWith(g.Key)).Take(3).ToList()
                })
                .OrderByDescending(x => x.Frequency)
                .ToList();

            Console.WriteLine($"Архетипы персонажей Johnny Depp ({deppRoles.Count} ролей):");
            foreach (var archetype in roleArchetypes)
            {
                Console.WriteLine($"{archetype.Archetype}: {archetype.Frequency} ролей");
                if (archetype.Examples.Any())
                {
                    Console.WriteLine($"  Примеры: {string.Join("; ", archetype.Examples)}");
                }
            }
        }
    }
}