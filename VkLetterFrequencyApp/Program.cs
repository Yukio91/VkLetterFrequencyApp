using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using VkLetterFrequencyApp.Utils;
using VkNet;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace VkLetterFrequencyApp
{
    class Program
    {
       static void Main(string[] args)
        {
            using (var api = new VkApi())
            {
                while (true)
                {
                    Console.WriteLine("Введите идентификатор приложения:");
                    if (UInt64.TryParse(Console.ReadLine(), out ulong appId))
                    {
                        Console.WriteLine("Введён неверный идентификатор приложения");

                        continue;
                    }

                    Console.WriteLine("Введите е-майл или номер телефона:");
                    var login = Console.ReadLine();

                    Console.WriteLine("Введите пароль:");
                    var password = ConsoleHelper.MaskingPassword();

                    try
                    {
                        api.Authorize(new ApiAuthParams
                        {
                            ApplicationId = appId,
                            Login = login,
                            Password = password,
                            Settings = Settings.Wall
                        });

                        break;
                    }
                    catch (VkApiAuthorizationException)
                    {
                        Console.WriteLine("Неверный логин или пароль");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при авторизации. {ex.Message}");
                    }
                }

                Console.WriteLine($"Успешно. Токен: {api.Token}");

                while (true)
                {
                    Console.WriteLine("Введите короткое имя страницы");
                    var screenName = Console.ReadLine();

                    try
                    {
                        var ownerId = VkHelper.GetObjectId(api, screenName, out string name);
                        if (!ownerId.HasValue)
                            continue;

                        string posts;
                        try
                        {
                            var wallGets = api.Wall.Get(new WallGetParams {OwnerId = ownerId, Count = 5});

                            posts = String.Join("", wallGets.WallPosts.Select(p => p.Text).ToArray());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при получении постов. {ex.Message}");

                            continue;
                        }

                        var freq = LettersHelper.Frequency(posts);
                        var json = JsonConvert.SerializeObject(freq, Formatting.Indented);

                       var message = $"{name}, статистика для последних 5 постов: {json}";
                        Console.WriteLine(message);

                        var result = VkHelper.WallPost(api, message);
                        Console.WriteLine(result);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}");
                    }
                }
            }
        }
    }
}
