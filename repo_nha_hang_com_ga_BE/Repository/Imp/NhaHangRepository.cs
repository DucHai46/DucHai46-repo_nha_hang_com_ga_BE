using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.NhaHang;
using repo_nha_hang_com_ga_BE.Models.Responds.NhaHang;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class NhaHangRepository : INhaHangRepository
{
    private readonly IMongoCollection<NhaHang> _collection;
    private readonly IMapper _mapper;

    public NhaHangRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<NhaHang>("NhaHang");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<NhaHangRespond>>> GetAllNhaHangs(RequestSearchNhaHang request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<NhaHang>.Filter.Empty;
            filter &= Builders<NhaHang>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenNhaHang))
            {
                filter &= Builders<NhaHang>.Filter.Regex(x => x.tenNhaHang, new BsonRegularExpression($".*{request.tenNhaHang}.*"));
            }

            var projection = Builders<NhaHang>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenNhaHang)
                .Include(x => x.diaChi)
                .Include(x => x.soDienThoai)
                .Include(x => x.email)
                .Include(x => x.website)
                .Include(x => x.logo)
                .Include(x => x.banner)
                .Include(x => x.moTa);

            var findOptions = new FindOptions<NhaHang, NhaHangRespond>
            {
                Projection = projection
            };

            if (request.IsPaging)
            {
                long totalRecords = await collection.CountDocumentsAsync(filter);

                int totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

                int currentPage = request.PageNumber;
                if (currentPage < 1) currentPage = 1;
                if (currentPage > totalPages) currentPage = totalPages;

                findOptions.Skip = (currentPage - 1) * request.PageSize;
                findOptions.Limit = request.PageSize;

                var cursor = await collection.FindAsync(filter, findOptions);
                var nhaHangs = await cursor.ToListAsync();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<NhaHangRespond>>
                {
                    Paging = pagingDetail,
                    Data = nhaHangs
                };

                return new RespondAPIPaging<List<NhaHangRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var nhaHangs = await cursor.ToListAsync();

                return new RespondAPIPaging<List<NhaHangRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<NhaHangRespond>>
                    {
                        Data = nhaHangs,
                        Paging = new PagingDetail(1, nhaHangs.Count, nhaHangs.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<NhaHangRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<NhaHangRespond>> GetNhaHangById(string id)
    {
        try
        {
            var nhaHang = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (nhaHang == null)
            {
                return new RespondAPI<NhaHangRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy nhà hàng với ID đã cung cấp."
                );
            }

            var nhaHangRespond = _mapper.Map<NhaHangRespond>(nhaHang);

            return new RespondAPI<NhaHangRespond>(
                ResultRespond.Succeeded,
                "Lấy nhà hàng thành công.",
                nhaHangRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<NhaHangRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<NhaHangRespond>> CreateNhaHang(RequestAddNhaHang request)
    {
        try
        {
            NhaHang newNhaHang = _mapper.Map<NhaHang>(request);

            newNhaHang.createdDate = DateTimeOffset.UtcNow;
            newNhaHang.updatedDate = DateTimeOffset.UtcNow;
            newNhaHang.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newDanhMucMonAn.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newNhaHang);

            var nhaHangRespond = _mapper.Map<NhaHangRespond>(newNhaHang);

            return new RespondAPI<NhaHangRespond>(
                ResultRespond.Succeeded,
                "Tạo nhà hàng thành công.",
                nhaHangRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<NhaHangRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo nhà hàng: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<NhaHangRespond>> UpdateNhaHang(string id, RequestUpdateNhaHang request)
    {
        try
        {
            var filter = Builders<NhaHang>.Filter.Eq(x => x.Id, id);
            filter &= Builders<NhaHang>.Filter.Eq(x => x.isDelete, false);
            var nhaHang = await _collection.Find(filter).FirstOrDefaultAsync();

            if (nhaHang == null)
            {
                return new RespondAPI<NhaHangRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy nhà hàng với ID đã cung cấp."
                );
            }

            _mapper.Map(request, nhaHang);

            nhaHang.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, nhaHang);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<NhaHangRespond>(
                    ResultRespond.Error,
                    "Cập nhật nhà hàng không thành công."
                );
            }

            var nhaHangRespond = _mapper.Map<NhaHangRespond>(nhaHang);

            return new RespondAPI<NhaHangRespond>(
                ResultRespond.Succeeded,
                "Cập nhật nhà hàng thành công.",
                nhaHangRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<NhaHangRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật nhà hàng: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteNhaHang(string id)
    {
        try
        {
            var existingNhaHang = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingNhaHang == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy nhà hàng để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa nhà hàng không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa nhà hàng thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa nhà hàng: {ex.Message}"
            );
        }
    }
}