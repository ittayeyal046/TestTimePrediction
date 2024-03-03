using AutoMapper;

namespace TTPService
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            // creation
            ConfigureForCreationDtos();
        }

        private void ConfigureForCreationDtos()
        {
        }
    }
}
