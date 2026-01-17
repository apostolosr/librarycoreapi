using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Services.Parties;

public interface IPartiesService
{
    Task<IEnumerable<PartyDto>> GetParties();
    Task<PartyDto> GetParty(int id);
    Task<PartyDto> CreateParty(CreatePartyDto createDto);
    Task<PartyDto> UpdateParty(int id, UpdatePartyDto updateDto);
    Task DeleteParty(int id);
}
