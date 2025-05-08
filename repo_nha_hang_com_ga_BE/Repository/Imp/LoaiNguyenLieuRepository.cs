using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiNguyenLieu;
using repo_nha_hang_com_ga_BE.Models.Responds.LoaiNguyenLieu;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class LoaiNguyenLieuRepository : ILoaiNguyenLieuRepository
{
    private readonly IMongoCollection<LoaiNguyenLieu> _collection;
    private readonly IMapper _mapper;
    private readonly IMongoCollection<DanhMucNguyenLieu> _collectionDanhMucNguyenLieu;

    public LoaiNguyenLieuRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<LoaiNguyenLieu>("LoaiNguyenLieu");
        _collectionDanhMucNguyenLieu = database.GetCollection<DanhMucNguyenLieu>("DanhMucNguyenLieu");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<LoaiNguyenLieuRespond>>> GetAllLoaiNguyenLieus(RequestSearchLoaiNguyenLieu request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<LoaiNguyenLieu>.Filter.Empty;
            filter &= Builders<LoaiNguyenLieu>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.danhMucNguyenLieuId))
            {
                filter &= Builders<LoaiNguyenLieu>.Filter.Eq(x => x.danhMucNguyenLieu, request.danhMucNguyenLieuId);
            }

            if (!string.IsNullOrEmpty(request.tenLoai))
            {
                filter &= Builders<LoaiNguyenLieu>.Filter.Regex(x => x.tenLoai, new BsonRegularExpression($".*{request.tenLoai}.*"));
            }

            var projection = Builders<LoaiNguyenLieu>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenLoai)
                .Include(x => x.danhMucNguyenLieu)
                .Include(x => x.moTa);

            var findOptions = new FindOptions<LoaiNguyenLieu, LoaiNguyenLieu>
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
                var loaiNguyenLieus = await cursor.ToListAsync();

                // Lấy danh sách ID loại bàn
                var danhMucNguyenLieuIds = loaiNguyenLieus.Select(x => x.danhMucNguyenLieu).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var danhMucNguyenLieuFilter = Builders<DanhMucNguyenLieu>.Filter.In(x => x.Id, danhMucNguyenLieuIds);
                var danhMucNguyenLieuProjection = Builders<DanhMucNguyenLieu>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenDanhMuc);
                var danhMucNguyenLieus = await _collectionDanhMucNguyenLieu.Find(danhMucNguyenLieuFilter)
                    .Project<DanhMucNguyenLieu>(danhMucNguyenLieuProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var danhMucNguyenLieuDict = danhMucNguyenLieus.ToDictionary(x => x.Id, x => x.tenDanhMuc);

                // Map dữ liệu
                var loaiNguyenLieuResponds = loaiNguyenLieus.Select(loaiNguyenLieu => new LoaiNguyenLieuRespond
                {
                    id = loaiNguyenLieu.Id,
                    tenLoai = loaiNguyenLieu.tenLoai,
                    danhMucNguyenLieu = new IdName
                    {
                        Id = loaiNguyenLieu.danhMucNguyenLieu,
                        Name = loaiNguyenLieu.danhMucNguyenLieu != null && danhMucNguyenLieuDict.ContainsKey(loaiNguyenLieu.danhMucNguyenLieu) ? danhMucNguyenLieuDict[loaiNguyenLieu.danhMucNguyenLieu] : null
                    },
                    moTa = loaiNguyenLieu.moTa
                }).ToList();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<LoaiNguyenLieuRespond>>
                {
                    Paging = pagingDetail,
                    Data = loaiNguyenLieuResponds
                };

                return new RespondAPIPaging<List<LoaiNguyenLieuRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var loaiNguyenLieus = await cursor.ToListAsync();

                // Lấy danh sách ID loại bàn
                var danhMucNguyenLieuIds = loaiNguyenLieus.Select(x => x.danhMucNguyenLieu).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var danhMucNguyenLieuFilter = Builders<DanhMucNguyenLieu>.Filter.In(x => x.Id, danhMucNguyenLieuIds);
                var danhMucNguyenLieuProjection = Builders<DanhMucNguyenLieu>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenDanhMuc);
                var danhMucNguyenLieus = await _collectionDanhMucNguyenLieu.Find(danhMucNguyenLieuFilter)
                    .Project<DanhMucNguyenLieu>(danhMucNguyenLieuProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var danhMucNguyenLieuDict = danhMucNguyenLieus.ToDictionary(x => x.Id, x => x.tenDanhMuc);

                // Map dữ liệu
                var loaiNguyenLieuResponds = loaiNguyenLieus.Select(loaiNguyenLieu => new LoaiNguyenLieuRespond
                {
                    id = loaiNguyenLieu.Id,
                    tenLoai = loaiNguyenLieu.tenLoai,
                    danhMucNguyenLieu = new IdName
                    {
                        Id = loaiNguyenLieu.danhMucNguyenLieu,
                        Name = loaiNguyenLieu.danhMucNguyenLieu != null && danhMucNguyenLieuDict.ContainsKey(loaiNguyenLieu.danhMucNguyenLieu) ? danhMucNguyenLieuDict[loaiNguyenLieu.danhMucNguyenLieu] : null
                    },
                    moTa = loaiNguyenLieu.moTa
                }).ToList();

                return new RespondAPIPaging<List<LoaiNguyenLieuRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<LoaiNguyenLieuRespond>>
                    {
                        Data = loaiNguyenLieuResponds,
                        Paging = new PagingDetail(1, loaiNguyenLieuResponds.Count, loaiNguyenLieuResponds.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<LoaiNguyenLieuRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<LoaiNguyenLieuRespond>> GetLoaiNguyenLieuById(string id)
    {
        try
        {
            var loaiNguyenLieu = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (loaiNguyenLieu == null)
            {
                return new RespondAPI<LoaiNguyenLieuRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy loại nguyên liệu với ID đã cung cấp."
                );
            }

            var danhMucNguyenLieu = await _collectionDanhMucNguyenLieu.Find(x => x.Id == loaiNguyenLieu.danhMucNguyenLieu).FirstOrDefaultAsync();
            var loaiNguyenLieuRespond = new LoaiNguyenLieuRespond
            {
                id = loaiNguyenLieu.Id,
                tenLoai = loaiNguyenLieu.tenLoai,
                danhMucNguyenLieu = new IdName
                {
                    Id = danhMucNguyenLieu.Id,
                    Name = danhMucNguyenLieu.tenDanhMuc
                },
                moTa = loaiNguyenLieu.moTa
            };

            return new RespondAPI<LoaiNguyenLieuRespond>(
                ResultRespond.Succeeded,
                "Lấy loại nguyên liệu thành công.",
                loaiNguyenLieuRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<LoaiNguyenLieuRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<LoaiNguyenLieuRespond>> CreateLoaiNguyenLieu(RequestAddLoaiNguyenLieu request)
    {
        try
        {
            LoaiNguyenLieu newLoaiNguyenLieu = _mapper.Map<LoaiNguyenLieu>(request);

            newLoaiNguyenLieu.createdDate = DateTimeOffset.UtcNow;
            newLoaiNguyenLieu.updatedDate = DateTimeOffset.UtcNow;
            newLoaiNguyenLieu.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newLoaiNguyenLieu.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newLoaiNguyenLieu);

            var danhMucNguyenLieu = await _collectionDanhMucNguyenLieu.Find(x => x.Id == newLoaiNguyenLieu.danhMucNguyenLieu).FirstOrDefaultAsync();
            var loaiNguyenLieuRespond = new LoaiNguyenLieuRespond
            {
                id = newLoaiNguyenLieu.Id,
                tenLoai = newLoaiNguyenLieu.tenLoai,
                danhMucNguyenLieu = new IdName
                {
                    Id = newLoaiNguyenLieu.danhMucNguyenLieu,
                    Name = danhMucNguyenLieu.tenDanhMuc
                },
                moTa = newLoaiNguyenLieu.moTa
            };
            return new RespondAPI<LoaiNguyenLieuRespond>(
                ResultRespond.Succeeded,
                "Tạo loại nguyên liệu thành công.",
                loaiNguyenLieuRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<LoaiNguyenLieuRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo loại nguyên liệu: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<LoaiNguyenLieuRespond>> UpdateLoaiNguyenLieu(string id, RequestUpdateLoaiNguyenLieu request)
    {
        try
        {
            var filter = Builders<LoaiNguyenLieu>.Filter.Eq(x => x.Id, id);
            filter &= Builders<LoaiNguyenLieu>.Filter.Eq(x => x.isDelete, false);
            var loaiNguyenLieu = await _collection.Find(filter).FirstOrDefaultAsync();

            if (loaiNguyenLieu == null)
            {
                return new RespondAPI<LoaiNguyenLieuRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy loại nguyên liệu với ID đã cung cấp."
                );
            }

            _mapper.Map(request, loaiNguyenLieu);

            loaiNguyenLieu.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, loaiNguyenLieu);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<LoaiNguyenLieuRespond>(
                    ResultRespond.Error,
                    "Cập nhật loại nguyên liệu không thành công."
                );
            }

            var danhMucNguyenLieu = await _collectionDanhMucNguyenLieu.Find(x => x.Id == loaiNguyenLieu.danhMucNguyenLieu).FirstOrDefaultAsync();
            var loaiNguyenLieuRespond = new LoaiNguyenLieuRespond
            {
                id = loaiNguyenLieu.Id,
                tenLoai = loaiNguyenLieu.tenLoai,
                danhMucNguyenLieu = new IdName
                {
                    Id = loaiNguyenLieu.danhMucNguyenLieu,
                    Name = danhMucNguyenLieu.tenDanhMuc
                },
                moTa = loaiNguyenLieu.moTa
            };
            return new RespondAPI<LoaiNguyenLieuRespond>(
                ResultRespond.Succeeded,
                "Cập nhật loại nguyên liệu thành công.",
                loaiNguyenLieuRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<LoaiNguyenLieuRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật loại nguyên liệu: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteLoaiNguyenLieu(string id)
    {
        try
        {
            var existingLoaiNguyenLieu = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingLoaiNguyenLieu == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy loại nguyên liệu để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa loại nguyên liệu không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa loại nguyên liệu thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa loại nguyên liệu: {ex.Message}"
            );
        }
    }
}