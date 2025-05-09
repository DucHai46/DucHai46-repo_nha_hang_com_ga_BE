using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.ThucDon;
using repo_nha_hang_com_ga_BE.Models.Responds.Combo;
using repo_nha_hang_com_ga_BE.Models.Responds.ThucDon;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class ThucDonRepository : IThucDonRepository
{
    private readonly IMongoCollection<ThucDon> _collection;
    private readonly IMongoCollection<Combo> _collectionCombo;
    private readonly IMongoCollection<LoaiMonAn> _collectionLoaiMonAn;
    private readonly IMongoCollection<MonAn> _collectionMonAn;
    private readonly IMapper _mapper;

    public ThucDonRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<ThucDon>("ThucDon");
        _collectionCombo = database.GetCollection<Combo>("Combo");
        _collectionLoaiMonAn = database.GetCollection<LoaiMonAn>("LoaiMonAn");
        _collectionMonAn = database.GetCollection<MonAn>("MonAn");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<ThucDonRespond>>> GetAllThucDons(RequestSearchThucDon request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<ThucDon>.Filter.Empty;
            filter &= Builders<ThucDon>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenThucDon))
            {
                filter &= Builders<ThucDon>.Filter.Regex(x => x.tenThucDon, new BsonRegularExpression($".*{request.tenThucDon}.*"));
            }

            if (!string.IsNullOrEmpty(request.loaiMonAnId))
            {
                filter &= Builders<ThucDon>.Filter.AnyEq("loaiMonAns.Id", request.loaiMonAnId);
            }

            if (!string.IsNullOrEmpty(request.comboId))
            {
                filter &= Builders<ThucDon>.Filter.ElemMatch(x => x.combos, Builders<ComboMenu>.Filter.Eq(y => y.id, request.comboId));
            }

            if (request.trangThai.HasValue)
            {
                filter &= Builders<ThucDon>.Filter.Eq(x => x.trangThai, request.trangThai);
            }

            var projection = Builders<ThucDon>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenThucDon)
                .Include(x => x.loaiMonAns)
                .Include(x => x.combos)
                .Include(x => x.trangThai);

            var findOptions = new FindOptions<ThucDon, ThucDon>
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
                var thucDons = await cursor.ToListAsync();

                var monAnDict = new Dictionary<string, string>();
                var loaiMonAnDict = new Dictionary<string, string>();
                var comboDict = new Dictionary<string, string>();

                var loaiMonAnIds = thucDons.SelectMany(x => x.loaiMonAns.Select(y => y.id)).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var loaiMonAnFilter = Builders<LoaiMonAn>.Filter.In(x => x.Id, loaiMonAnIds);
                var loaiMonAnProjection = Builders<LoaiMonAn>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiMonAns = await _collectionLoaiMonAn.Find(loaiMonAnFilter)
                    .Project<LoaiMonAn>(loaiMonAnProjection)
                    .ToListAsync();

                loaiMonAnDict = loaiMonAns.ToDictionary(x => x.Id, x => x.tenLoai);

                List<MonAn> monAns = new List<MonAn>();
                foreach (var thucDon in thucDons)
                {
                    var monAnIds = thucDon.loaiMonAns.SelectMany(x => x.monAns.Select(y => y.id)).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                    var monAnFilter = Builders<MonAn>.Filter.In(x => x.Id, monAnIds);
                    var monAnProjection = Builders<MonAn>.Projection
                        .Include(x => x.Id)
                        .Include(x => x.tenMonAn)
                        .Include(x => x.hinhAnh)
                        .Include(x => x.giaTien)
                        .Include(x => x.moTa);
                    var newMonAns = await _collectionMonAn.Find(monAnFilter)
                        .Project<MonAn>(monAnProjection)
                        .ToListAsync();

                    var uniqueMonAns = newMonAns.Where(x => !monAns.Any(y => y.Id == x.Id));
                    monAns.AddRange(uniqueMonAns);

                    var newDict = monAns.ToDictionary(x => x.Id, x => x.tenMonAn);
                    foreach (var item in newDict)
                    {
                        if (!monAnDict.ContainsKey(item.Key))
                        {
                            monAnDict.Add(item.Key, item.Value);
                        }
                    }
                }

                var comboIds = thucDons.SelectMany(x => x.combos.Select(y => y.id)).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var comboFilter = Builders<Combo>.Filter.In(x => x.Id, comboIds);
                var comboProjection = Builders<Combo>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenCombo)
                    .Include(x => x.hinhAnh)
                    .Include(x => x.giaTien)
                    .Include(x => x.moTa);
                var combos = await _collectionCombo.Find(comboFilter)
                    .Project<Combo>(comboProjection)
                    .ToListAsync();

                var thucDonResponds = thucDons.Select(x => new ThucDonRespond
                {
                    id = x.Id,
                    tenThucDon = x.tenThucDon,
                    loaiMonAns = x.loaiMonAns.Select(y => new LoaiMonAnMenuRespond
                    {
                        Id = y.id,
                        Name = loaiMonAnDict.ContainsKey(y.id) ? loaiMonAnDict[y.id] : null,
                        monAns = y.monAns.Select(z => new MonAnMenuRespond
                        {
                            id = z.id,
                            tenMonAn = monAnDict.ContainsKey(z.id) ? monAnDict[z.id] : null,
                            hinhAnh = monAnDict.ContainsKey(z.id) ? monAns.FirstOrDefault(m => m.Id == z.id)?.hinhAnh : null,
                            giaTien = monAnDict.ContainsKey(z.id) ? monAns.FirstOrDefault(m => m.Id == z.id)?.giaTien : null,
                            moTa = monAnDict.ContainsKey(z.id) ? monAns.FirstOrDefault(m => m.Id == z.id)?.moTa : null
                        }).ToList(),
                        moTa = y.moTa
                    }).ToList(),
                    combos = x.combos.Select(y => new ComboMenuRespond
                    {
                        Id = y.id,
                        Name = comboDict.ContainsKey(y.id) ? comboDict[y.id] : null,
                        hinhAnh = comboDict.ContainsKey(y.id) ? combos.FirstOrDefault(m => m.Id == y.id)?.hinhAnh : null,
                        giaTien = comboDict.ContainsKey(y.id) ? combos.FirstOrDefault(m => m.Id == y.id)?.giaTien : null,
                        moTa = comboDict.ContainsKey(y.id) ? combos.FirstOrDefault(m => m.Id == y.id)?.moTa : null
                    }).ToList(),
                }).ToList();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<ThucDonRespond>>
                {
                    Paging = pagingDetail,
                    Data = thucDonResponds
                };

                return new RespondAPIPaging<List<ThucDonRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var thucDons = await cursor.ToListAsync();

                                var monAnDict = new Dictionary<string, string>();
                var loaiMonAnDict = new Dictionary<string, string>();
                var comboDict = new Dictionary<string, string>();

                var loaiMonAnIds = thucDons.SelectMany(x => x.loaiMonAns.Select(y => y.id)).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var loaiMonAnFilter = Builders<LoaiMonAn>.Filter.In(x => x.Id, loaiMonAnIds);
                var loaiMonAnProjection = Builders<LoaiMonAn>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiMonAns = await _collectionLoaiMonAn.Find(loaiMonAnFilter)
                    .Project<LoaiMonAn>(loaiMonAnProjection)
                    .ToListAsync();

                loaiMonAnDict = loaiMonAns.ToDictionary(x => x.Id, x => x.tenLoai);

                List<MonAn> monAns = new List<MonAn>();
                foreach (var thucDon in thucDons)
                {
                    var monAnIds = thucDon.loaiMonAns.SelectMany(x => x.monAns.Select(y => y.id)).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                    var monAnFilter = Builders<MonAn>.Filter.In(x => x.Id, monAnIds);
                    var monAnProjection = Builders<MonAn>.Projection
                        .Include(x => x.Id)
                        .Include(x => x.tenMonAn)
                        .Include(x => x.hinhAnh)
                        .Include(x => x.giaTien)
                        .Include(x => x.moTa);
                    var newMonAns = await _collectionMonAn.Find(monAnFilter)
                        .Project<MonAn>(monAnProjection)
                        .ToListAsync();

                    var uniqueMonAns = newMonAns.Where(x => !monAns.Any(y => y.Id == x.Id));
                    monAns.AddRange(uniqueMonAns);

                    var newDict = monAns.ToDictionary(x => x.Id, x => x.tenMonAn);
                    foreach (var item in newDict)
                    {
                        if (!monAnDict.ContainsKey(item.Key))
                        {
                            monAnDict.Add(item.Key, item.Value);
                        }
                    }
                }

                var comboIds = thucDons.SelectMany(x => x.combos.Select(y => y.id)).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var comboFilter = Builders<Combo>.Filter.In(x => x.Id, comboIds);
                var comboProjection = Builders<Combo>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenCombo)
                    .Include(x => x.hinhAnh)
                    .Include(x => x.giaTien)
                    .Include(x => x.moTa);
                var combos = await _collectionCombo.Find(comboFilter)
                    .Project<Combo>(comboProjection)
                    .ToListAsync();

                var thucDonResponds = thucDons.Select(x => new ThucDonRespond
                {
                    id = x.Id,
                    tenThucDon = x.tenThucDon,
                    loaiMonAns = x.loaiMonAns.Select(y => new LoaiMonAnMenuRespond
                    {
                        Id = y.id,
                        Name = loaiMonAnDict.ContainsKey(y.id) ? loaiMonAnDict[y.id] : null,
                        monAns = y.monAns.Select(z => new MonAnMenuRespond
                        {
                            id = z.id,
                            tenMonAn = monAnDict.ContainsKey(z.id) ? monAnDict[z.id] : null,
                            hinhAnh = monAnDict.ContainsKey(z.id) ? monAns.FirstOrDefault(m => m.Id == z.id)?.hinhAnh : null,
                            giaTien = monAnDict.ContainsKey(z.id) ? monAns.FirstOrDefault(m => m.Id == z.id)?.giaTien : null,
                            moTa = monAnDict.ContainsKey(z.id) ? monAns.FirstOrDefault(m => m.Id == z.id)?.moTa : null
                        }).ToList(),
                        moTa = y.moTa
                    }).ToList(),
                    combos = x.combos.Select(y => new ComboMenuRespond
                    {
                        Id = y.id,
                        Name = comboDict.ContainsKey(y.id) ? comboDict[y.id] : null,
                        hinhAnh = comboDict.ContainsKey(y.id) ? combos.FirstOrDefault(m => m.Id == y.id)?.hinhAnh : null,
                        giaTien = comboDict.ContainsKey(y.id) ? combos.FirstOrDefault(m => m.Id == y.id)?.giaTien : null,
                        moTa = comboDict.ContainsKey(y.id) ? combos.FirstOrDefault(m => m.Id == y.id)?.moTa : null
                    }).ToList(),
                }).ToList();

                return new RespondAPIPaging<List<ThucDonRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<ThucDonRespond>>
                    {
                        Data = thucDonResponds,
                        Paging = new PagingDetail(1, thucDonResponds.Count, thucDonResponds.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<ThucDonRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<ThucDonRespond>> GetThucDonById(string id)
    {
        try
        {
            var thucDon = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (thucDon == null)
            {
                return new RespondAPI<ThucDonRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy thực đơn với ID đã cung cấp."
                );
            }

            var monAnDict = new Dictionary<string, string>();
            var loaiMonAnDict = new Dictionary<string, string>();

            var loaiMonAnIds = thucDon.loaiMonAns.Select(x => x.id).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var loaiMonAnFilter = Builders<LoaiMonAn>.Filter.In(x => x.Id, loaiMonAnIds);
            var loaiMonAnProjection = Builders<LoaiMonAn>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenLoai);
            var loaiMonAns = await _collectionLoaiMonAn.Find(loaiMonAnFilter)
                .Project<LoaiMonAn>(loaiMonAnProjection)
                .ToListAsync();

            loaiMonAnDict = loaiMonAns.ToDictionary(x => x.Id, x => x.tenLoai);

            List<MonAn> monAns = new List<MonAn>();

            var monAnIds = thucDon.loaiMonAns.SelectMany(x => x.monAns.Select(y => y.id)).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var monAnFilter = Builders<MonAn>.Filter.In(x => x.Id, monAnIds);
            var monAnProjection = Builders<MonAn>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenMonAn)
                .Include(x => x.hinhAnh)
                .Include(x => x.giaTien)
                .Include(x => x.moTa);
            monAns = await _collectionMonAn.Find(monAnFilter)
                .Project<MonAn>(monAnProjection)
                .ToListAsync();

            monAnDict = monAns.ToDictionary(x => x.Id, x => x.tenMonAn);

            var comboIds = thucDon.combos.Select(x => x.id).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var comboFilter = Builders<Combo>.Filter.In(x => x.Id, comboIds);
            var comboProjection = Builders<Combo>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenCombo)
                .Include(x => x.hinhAnh)
                .Include(x => x.giaTien)
                .Include(x => x.moTa);
            var combos = await _collectionCombo.Find(comboFilter)
                .Project<Combo>(comboProjection)
                .ToListAsync();

            var comboDict = combos.ToDictionary(x => x.Id, x => x.tenCombo);

            var thucDonRespond = new ThucDonRespond
            {
                id = thucDon.Id,
                tenThucDon = thucDon.tenThucDon,
                loaiMonAns = thucDon.loaiMonAns.Select(x => new LoaiMonAnMenuRespond
                {
                    Id = x.id,
                    Name = loaiMonAnDict.ContainsKey(x.id) ? loaiMonAnDict[x.id] : null,
                    monAns = x.monAns.Select(y => new MonAnMenuRespond
                    {
                        id = y.id,
                        tenMonAn = monAnDict.ContainsKey(y.id) ? monAnDict[y.id] : null,
                        hinhAnh = monAnDict.ContainsKey(y.id) ? monAns.FirstOrDefault(m => m.Id == y.id)?.hinhAnh : null,
                        giaTien = monAnDict.ContainsKey(y.id) ? monAns.FirstOrDefault(m => m.Id == y.id)?.giaTien : null,
                        moTa = monAnDict.ContainsKey(y.id) ? monAns.FirstOrDefault(m => m.Id == y.id)?.moTa : null
                    }).ToList(),
                    moTa = x.moTa
                }).ToList(),
                combos = thucDon.combos.Select(x => new ComboMenuRespond
                {
                    Id = x.id,
                    Name = comboDict.ContainsKey(x.id) ? comboDict[x.id] : null,
                    hinhAnh = comboDict.ContainsKey(x.id) ? combos.FirstOrDefault(m => m.Id == x.id)?.hinhAnh : null,
                    giaTien = comboDict.ContainsKey(x.id) ? combos.FirstOrDefault(m => m.Id == x.id)?.giaTien : null,
                    moTa = comboDict.ContainsKey(x.id) ? combos.FirstOrDefault(m => m.Id == x.id)?.moTa : null
                }).ToList(),
            };

            return new RespondAPI<ThucDonRespond>(
                ResultRespond.Succeeded,
                "Lấy thực đơn thành công.",
                thucDonRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<ThucDonRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<ThucDonRespond>> CreateThucDon(RequestAddThucDon request)
    {
        try
        {
            ThucDon newThucDon = _mapper.Map<ThucDon>(request);

            newThucDon.createdDate = DateTimeOffset.UtcNow;
            newThucDon.updatedDate = DateTimeOffset.UtcNow;
            newThucDon.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newDanhMucMonAn.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newThucDon);

           var monAnDict = new Dictionary<string, string>();
            var loaiMonAnDict = new Dictionary<string, string>();

            var loaiMonAnIds = newThucDon.loaiMonAns.Select(x => x.id).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var loaiMonAnFilter = Builders<LoaiMonAn>.Filter.In(x => x.Id, loaiMonAnIds);
            var loaiMonAnProjection = Builders<LoaiMonAn>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenLoai);
            var loaiMonAns = await _collectionLoaiMonAn.Find(loaiMonAnFilter)
                .Project<LoaiMonAn>(loaiMonAnProjection)
                .ToListAsync();

            loaiMonAnDict = loaiMonAns.ToDictionary(x => x.Id, x => x.tenLoai);

            List<MonAn> monAns = new List<MonAn>();

            var monAnIds = newThucDon.loaiMonAns.SelectMany(x => x.monAns.Select(y => y.id)).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var monAnFilter = Builders<MonAn>.Filter.In(x => x.Id, monAnIds);
            var monAnProjection = Builders<MonAn>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenMonAn)
                .Include(x => x.hinhAnh)
                .Include(x => x.giaTien)
                .Include(x => x.moTa);
            monAns = await _collectionMonAn.Find(monAnFilter)
                .Project<MonAn>(monAnProjection)
                .ToListAsync();

            monAnDict = monAns.ToDictionary(x => x.Id, x => x.tenMonAn);

            var comboIds = newThucDon.combos.Select(x => x.id).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var comboFilter = Builders<Combo>.Filter.In(x => x.Id, comboIds);
            var comboProjection = Builders<Combo>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenCombo)
                .Include(x => x.hinhAnh)
                .Include(x => x.giaTien)
                .Include(x => x.moTa);
            var combos = await _collectionCombo.Find(comboFilter)
                .Project<Combo>(comboProjection)
                .ToListAsync();

            var comboDict = combos.ToDictionary(x => x.Id, x => x.tenCombo);

            var thucDonRespond = new ThucDonRespond
            {
                id = newThucDon.Id,
                tenThucDon = newThucDon.tenThucDon,
                loaiMonAns = newThucDon.loaiMonAns.Select(x => new LoaiMonAnMenuRespond
                {
                    Id = x.id,
                    Name = loaiMonAnDict.ContainsKey(x.id) ? loaiMonAnDict[x.id] : null,
                    monAns = x.monAns.Select(y => new MonAnMenuRespond
                    {
                        id = y.id,
                        tenMonAn = monAnDict.ContainsKey(y.id) ? monAnDict[y.id] : null,
                        hinhAnh = monAnDict.ContainsKey(y.id) ? monAns.FirstOrDefault(m => m.Id == y.id)?.hinhAnh : null,
                        giaTien = monAnDict.ContainsKey(y.id) ? monAns.FirstOrDefault(m => m.Id == y.id)?.giaTien : null,
                        moTa = monAnDict.ContainsKey(y.id) ? monAns.FirstOrDefault(m => m.Id == y.id)?.moTa : null
                    }).ToList(),
                    moTa = x.moTa
                }).ToList(),
                combos = newThucDon.combos.Select(x => new ComboMenuRespond
                {
                    Id = x.id,
                    Name = comboDict.ContainsKey(x.id) ? comboDict[x.id] : null,
                    hinhAnh = comboDict.ContainsKey(x.id) ? combos.FirstOrDefault(m => m.Id == x.id)?.hinhAnh : null,
                    giaTien = comboDict.ContainsKey(x.id) ? combos.FirstOrDefault(m => m.Id == x.id)?.giaTien : null,
                    moTa = comboDict.ContainsKey(x.id) ? combos.FirstOrDefault(m => m.Id == x.id)?.moTa : null
                }).ToList(),
            };

            return new RespondAPI<ThucDonRespond>(
                ResultRespond.Succeeded,
                "Tạo thực đơn thành công.",
                thucDonRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<ThucDonRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo thực đơn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<ThucDonRespond>> UpdateThucDon(string id, RequestUpdateThucDon request)
    {
        try
        {
            var filter = Builders<ThucDon>.Filter.Eq(x => x.Id, id);
            filter &= Builders<ThucDon>.Filter.Eq(x => x.isDelete, false);
            var thucDon = await _collection.Find(filter).FirstOrDefaultAsync();

            if (thucDon == null)
            {
                return new RespondAPI<ThucDonRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy thực đơn với ID đã cung cấp."
                );
            }

            _mapper.Map(request, thucDon);

            thucDon.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, thucDon);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<ThucDonRespond>(
                    ResultRespond.Error,
                    "Cập nhật thực đơn không thành công."
                );
            }

           var monAnDict = new Dictionary<string, string>();
            var loaiMonAnDict = new Dictionary<string, string>();

            var loaiMonAnIds = thucDon.loaiMonAns.Select(x => x.id).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var loaiMonAnFilter = Builders<LoaiMonAn>.Filter.In(x => x.Id, loaiMonAnIds);
            var loaiMonAnProjection = Builders<LoaiMonAn>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenLoai);
            var loaiMonAns = await _collectionLoaiMonAn.Find(loaiMonAnFilter)
                .Project<LoaiMonAn>(loaiMonAnProjection)
                .ToListAsync();

            loaiMonAnDict = loaiMonAns.ToDictionary(x => x.Id, x => x.tenLoai);

            List<MonAn> monAns = new List<MonAn>();

            var monAnIds = thucDon.loaiMonAns.SelectMany(x => x.monAns.Select(y => y.id)).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var monAnFilter = Builders<MonAn>.Filter.In(x => x.Id, monAnIds);
            var monAnProjection = Builders<MonAn>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenMonAn)
                .Include(x => x.hinhAnh)
                .Include(x => x.giaTien)
                .Include(x => x.moTa);
            monAns = await _collectionMonAn.Find(monAnFilter)
                .Project<MonAn>(monAnProjection)
                .ToListAsync();

            monAnDict = monAns.ToDictionary(x => x.Id, x => x.tenMonAn);

            var comboIds = thucDon.combos.Select(x => x.id).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var comboFilter = Builders<Combo>.Filter.In(x => x.Id, comboIds);
            var comboProjection = Builders<Combo>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenCombo)
                .Include(x => x.hinhAnh)
                .Include(x => x.giaTien)
                .Include(x => x.moTa);
            var combos = await _collectionCombo.Find(comboFilter)
                .Project<Combo>(comboProjection)
                .ToListAsync();

            var comboDict = combos.ToDictionary(x => x.Id, x => x.tenCombo);

            var thucDonRespond = new ThucDonRespond
            {
                id = thucDon.Id,
                tenThucDon = thucDon.tenThucDon,
                loaiMonAns = thucDon.loaiMonAns.Select(x => new LoaiMonAnMenuRespond
                {
                    Id = x.id,
                    Name = loaiMonAnDict.ContainsKey(x.id) ? loaiMonAnDict[x.id] : null,
                    monAns = x.monAns.Select(y => new MonAnMenuRespond
                    {
                        id = y.id,
                        tenMonAn = monAnDict.ContainsKey(y.id) ? monAnDict[y.id] : null,
                        hinhAnh = monAnDict.ContainsKey(y.id) ? monAns.FirstOrDefault(m => m.Id == y.id)?.hinhAnh : null,
                        giaTien = monAnDict.ContainsKey(y.id) ? monAns.FirstOrDefault(m => m.Id == y.id)?.giaTien : null,
                        moTa = monAnDict.ContainsKey(y.id) ? monAns.FirstOrDefault(m => m.Id == y.id)?.moTa : null
                    }).ToList(),
                    moTa = x.moTa
                }).ToList(),
                combos = thucDon.combos.Select(x => new ComboMenuRespond
                {
                    Id = x.id,
                    Name = comboDict.ContainsKey(x.id) ? comboDict[x.id] : null,
                    hinhAnh = comboDict.ContainsKey(x.id) ? combos.FirstOrDefault(m => m.Id == x.id)?.hinhAnh : null,
                    giaTien = comboDict.ContainsKey(x.id) ? combos.FirstOrDefault(m => m.Id == x.id)?.giaTien : null,
                    moTa = comboDict.ContainsKey(x.id) ? combos.FirstOrDefault(m => m.Id == x.id)?.moTa : null
                }).ToList(),
            };

            return new RespondAPI<ThucDonRespond>(
                ResultRespond.Succeeded,
                "Cập nhật thực đơn thành công.",
                thucDonRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<ThucDonRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật thực đơn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteThucDon(string id)
    {
        try
        {
            var existingThucDon = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingThucDon == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy thực đơn để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa thực đơn không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa thực đơn thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa thực đơn: {ex.Message}"
            );
        }
    }
}