using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using AutoMapper;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;

using MongoDB.Bson;

using Microsoft.Extensions.Options;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using System.ComponentModel;
using repo_nha_hang_com_ga_BE.Models.Responds.PhuPhi;
using repo_nha_hang_com_ga_BE.Models.Requests;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class PhuPhiRepository : IPhuPhiRepository
{
    private readonly IMongoCollection<PhuPhi> _collection;
    private readonly IMapper _mapper;

    public PhuPhiRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<PhuPhi>("PhuPhi");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<PhuPhiRespond>>> GetAllPhuPhis(RequestSearchPhuPhi request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<PhuPhi>.Filter.Empty;
            filter &= Builders<PhuPhi>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenPhuPhi))
            {
                filter &= Builders<PhuPhi>.Filter.Regex(x => x.tenPhuPhi, new BsonRegularExpression($".*{request.tenPhuPhi}.*"));

            }


            var projection = Builders<PhuPhi>.Projection
                .Include(x => x.tenPhuPhi)
                .Include(x => x.giaTri)
                .Include(x => x.moTa)
                .Include(x => x.trangThai);

            var findOptions = new FindOptions<PhuPhi, PhuPhiRespond>
            {
                Projection = projection
            };

            if (request.IsPaging)
            {
                long totalRecord = await collection.CountDocumentsAsync(filter);
                int totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);

                int currentPage = request.PageNumber;
                if (currentPage < 1) currentPage = 1;
                if (currentPage > totalPages) currentPage = totalPages;

                findOptions.Skip = (currentPage - 1) * request.PageSize;
                findOptions.Limit = request.PageSize;

                var cursor = await collection.FindAsync(filter, findOptions);
                var PhuPhis = await cursor.ToListAsync();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecord);
                var pagingResponse = new PagingResponse<List<PhuPhiRespond>>
                {
                    Paging = pagingDetail,
                    Data = PhuPhis
                };

                return new RespondAPIPaging<List<PhuPhiRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var PhuPhis = await cursor.ToListAsync();

                return new RespondAPIPaging<List<PhuPhiRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<PhuPhiRespond>>
                    {
                        Data = PhuPhis,
                        Paging = new PagingDetail(1, PhuPhis.Count, PhuPhis.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<PhuPhiRespond>>(
                ResultRespond.Failed,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<PhuPhiRespond>> GetPhuPhiById(string id)
    {
        try
        {
            var PhuPhi = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (PhuPhi == null)
            {
                return new RespondAPI<PhuPhiRespond>(
                    ResultRespond.Failed,
                    message: "Phụ phí không tồn tại"
                );
            }

            var PhuPhiRespond = _mapper.Map<PhuPhiRespond>(PhuPhi);

            return new RespondAPI<PhuPhiRespond>(
                ResultRespond.Succeeded,
                "Lấy thông tin Phụ phí thành công",
                PhuPhiRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<PhuPhiRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<PhuPhiRespond>> CreatePhuPhi(RequestAddPhuPhi request)
    {
        try
        {
            PhuPhi newPhuPhi = _mapper.Map<PhuPhi>(request);

            newPhuPhi.isDelete = false;
            newPhuPhi.createdDate = DateTime.Now;
            newPhuPhi.updatedDate = DateTime.Now;

            await _collection.InsertOneAsync(newPhuPhi);

            var PhuPhiRespond = _mapper.Map<PhuPhiRespond>(newPhuPhi);

            return new RespondAPI<PhuPhiRespond>(
                ResultRespond.Succeeded,
                "Thêm Phụ phí thành công",
                PhuPhiRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<PhuPhiRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<PhuPhiRespond>> UpdatePhuPhi(string id, RequestUpdatePhuPhi request)
    {
        try
        {
            var filter = Builders<PhuPhi>.Filter.Eq(x => x.Id, id);
            filter &= Builders<PhuPhi>.Filter.Eq(x => x.isDelete, false);
            var PhuPhi = await _collection.Find(filter).FirstOrDefaultAsync();

            if (PhuPhi == null)
            {
                return new RespondAPI<PhuPhiRespond>(
                    ResultRespond.NotFound,
                    "Phụ phí không tồn tại."
                );
            }

            _mapper.Map(request, PhuPhi);

            PhuPhi.updatedDate = DateTimeOffset.UtcNow;

            var updateResult = await _collection.ReplaceOneAsync(filter, PhuPhi);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<PhuPhiRespond>(
                    ResultRespond.Error,
                    "Cập nhật Phụ phí không thành công."
                );
            }
            var PhuPhiRespond = _mapper.Map<PhuPhiRespond>(PhuPhi);

            return new RespondAPI<PhuPhiRespond>(
                ResultRespond.Succeeded,
                "Cập nhật Phụ phí thành công",
                PhuPhiRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<PhuPhiRespond>
            (
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật thông tin Phụ phí: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeletePhuPhi(string id)
    {
        try
        {
            var existingPhuPhi = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (existingPhuPhi == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    message: "Phụ phí không tồn tại"
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa Phụ phí không thành công."
                );
            }
            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa Phụ phí thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa Phụ phí: {ex.Message}"
            );
        }
    }
}