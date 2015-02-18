using System;
using MediaBrowser.Library;
using MediaBrowser.Library.Entities;
using MediaBrowser.Library.Util;
using MediaBrowser.Model.Dto;
using Microsoft.MediaCenter.UI;

namespace Subdued.APICalls
{
    class GetAPIItems
    {
        //Instantiate the APIQueries Class
        private static readonly APIQueries APIQuery = new APIQueries();

        internal GetAPIItems()
        {
        }

        //
        public static ArrayListDataSet GetNextUpSet()
        {
            //GUID taken from MBC Kernel
            Guid id = Kernel.CurrentUser.Id;
            ArrayListDataSet nextUpSet = new ArrayListDataSet();

            //Enumerate thru the list Items in the API call(add the UserId to allow for custom filtering)
            //
            foreach (BaseItemDto dto in APIQuery.NextUpAPIQuery(id).Items)
            {
                Item item = GetNextUpItem(dto);
                nextUpSet.Add(item);
            }
            return nextUpSet;
        }

        //Gets the NextUp item as long as it's of type episode
        private static Item GetNextUpItem(BaseItemDto dto)
        {
            //Retrieves the item based on the items guid
            BaseItem baseItem = Kernel.Instance.MB3ApiRepository.RetrieveItem(new Guid(dto.Id));
            //If the call to the api returns empty, catch it here.
            if (baseItem == null)
            {
                return null;
            }
            //Ensure that we only return episodes
            //Lets tell the Kernel what we want to return and create that item.
            Item episodeItem = ItemFactory.Instance.Create(baseItem);
            if (episodeItem.BaseItem is Episode)
            {
                TVHelper.CreateEpisodeParents(episodeItem);
            }
            return episodeItem;
        }
    }
}
