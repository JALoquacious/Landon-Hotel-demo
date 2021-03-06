﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using LandonApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LandonApi.Services
{
    public class DefaultRoomService : IRoomService
    {
        private readonly HotelApiContext _context;

        public DefaultRoomService(HotelApiContext context)
        {
            _context = context;
        }

        public async Task<Room> GetRoomAsync(Guid roomId, CancellationToken ct)
        {
            var entity = await _context.Rooms.SingleOrDefaultAsync(r => r.Id == roomId, ct);
            if (entity == null) return null;

            return Mapper.Map<Room>(entity);
        }

        public async Task<PagedResults<Room>> GetRoomsAsync(
            PagingOptions pagingOptions,
            SortOptions<Room, RoomEntity> sortOptions,
            SearchOptions<Room, RoomEntity> searchOptions,
            CancellationToken ct)
        {
            IQueryable<RoomEntity> query = _context.Rooms;
            // Apply search before sort
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);

            var size = await query.CountAsync(ct);

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<Room>()
                .ToArrayAsync(ct);

            return new PagedResults<Room>
            {
                Items = items,
                TotalSize = size
            };
        }
    }
}
