using System;
using System.Linq;
using VkNet;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;

namespace VkLetterFrequencyApp.Utils
{
    public static class VkHelper
    {
        public static string WallPost(VkApi api, string message)
        {
            try
            {
                var result = api.Wall.Post(new WallPostParams
                {
                    Message = message
                });

                switch (result)
                {
                    case 214:
                        return "Публикация запрещена. Превышен лимит на число публикаций в сутки, " +
                               "либо на указанное время уже запланирована другая запись, " +
                               "либо для текущего пользователя недоступно размещение записи на этой стене";
                    case 219:
                        return "Рекламный пост уже недавно публиковался";

                    case 220:
                        return "Слишком много получателей";

                    case 222:
                        return "Запрещено размещать ссылки";

                    case 224:
                        return "Превышен лимит рекламных записей";
                }

                return result > 0 ? "Сообщение опубликовано" : "Ошибка публикации сообщения";
            }
            catch (Exception ex)
            {
                return $"Ошибка при публикации сообщения. {ex.Message}";
            }
        }

        public static long? GetObjectId(VkApi api, string screenName, out string displayName)
        {
            displayName = String.Empty;

            try
            {
                var vkObj = api.Utils.ResolveScreenName(screenName);
                if (!vkObj.Id.HasValue)
                {
                    Console.WriteLine("Страница не найдена или не является пользователем/группой");

                    return null;
                }

                switch (vkObj.Type)
                {
                    case VkObjectType.User:
                        var user = api.Users.Get(new[] {screenName}).FirstOrDefault();

                        displayName = $"{user?.FirstName} {user?.LastName}";
                        break;
                    case VkObjectType.Group:
                        var group = api.Groups.GetById(new[] {screenName}, screenName, GroupsFields.Description)
                            .FirstOrDefault();

                        displayName = group?.Name;
                        break;
                    case VkObjectType.Application:
                        return null;
                }

                return vkObj.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");

                return null;
            }
        }
    }
}
