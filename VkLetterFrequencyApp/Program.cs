using Newtonsoft.Json;
using System;
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
        private const int AppId = 123456; //todo: временнный идентификатор

        static void Main(string[] args)
        {
            using (var api = new VkApi())
            {

                while (true)
                {
                    Console.WriteLine("Введите е-майл или номер телефона:");
                    var login = Console.ReadLine();

                    Console.WriteLine("Введите пароль:");
                    var password = ConsoleHelper.MaskingPassword();

                    try
                    {
                        api.Authorize(new ApiAuthParams
                        {
                            ApplicationId = AppId,
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
                        Console.WriteLine($"Ошибка. {ex.Message}");
                    }
                }

                Console.WriteLine($"Успешно. Токен: {api.Token}");


                while (true)
                {
                    Console.WriteLine("Введите ид страницы");
                    var vkId = Console.ReadLine();

                    var vkObj = api.Utils.ResolveScreenName(vkId);
                    if (!vkObj.Id.HasValue || vkObj.Type == VkNet.Enums.VkObjectType.Application)
                    {
                        Console.WriteLine("Страница не найдена");

                        continue;
                    }

                    var ownerId = vkObj.Id.Value;

                    string posts="";
                    try
                    {
                        var wallGets = api.Wall.Get(new WallGetParams {OwnerId = ownerId, Count = 5});

                        posts = String.Join("", wallGets.WallPosts.Select(p => p.Text).ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка. {ex.Message}");

                        continue;
                    }

                    var letters = posts.ToLower().Where(Char.IsLetter).OrderBy(c => c).ToList();
                    double countAll = letters.Count;
                    var freq = letters.GroupBy(c => c)
                        .ToDictionary(g => g.Key, g => Math.Round(g.Count() / countAll, 5));

                    var json = JsonConvert.SerializeObject(freq, Formatting.Indented);

                    var message = "";
                    if (vkObj.Type == VkObjectType.User)
                    {
                        var user = api.Users.Get(new[] {ownerId}).FirstOrDefault();
                        message = $"{user?.FirstName} {user?.LastName}, статистика для последних 5 постов: {json}";
                    }
                    else if (vkObj.Type == VkObjectType.Group)
                    {
                        var group = api.Groups.GetById(new[] {vkId}, vkId, GroupsFields.Description).FirstOrDefault();
                        message = $"{group?.Name}, статистика для последних 5 постов: {json}";
                    }

                    Console.WriteLine(message);

                    var result = api.Wall.Post(new WallPostParams
                    {
                        Message = message
                    });

                    switch (result)
                    {
                        case 214:
                            Console.WriteLine("Публикация запрещена. Превышен лимит на число публикаций в сутки, " +
                                              "либо на указанное время уже запланирована другая запись, " +
                                              "либо для текущего пользователя недоступно размещение записи на этой стене");
                            break;
                        case 219:
                            Console.WriteLine("Рекламный пост уже недавно публиковался");
                            break;
                        case 220:
                            Console.WriteLine("Слишком много получателей");
                            break;
                        case 222:
                            Console.WriteLine("Запрещено размещать ссылки");
                            break;
                        case 224:
                            Console.WriteLine("Превышен лимит рекламных записей");
                            break;
                        default:
                            Console.WriteLine("Успешно");
                            break;
                    }
                }
            }
        }
    }
}
