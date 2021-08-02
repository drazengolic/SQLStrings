-- Name: SearchArtists
-- Query for searching artists.
-- Another comment line
select
  a.id,
  a.name,
  a.cover_photo,
  count(distinct ph.id) as rating
from
  artist a
  inner join track_artist ta on ta.artist_id = a.id
  left join play_history ph on ph.track_id = ta.track_id
where
  a.name ilike "%sample text%"
  and a.active = true
group by
  a.id
order by
  rating desc
limit 5;

-- Name: GetAllArtists
--
-- Retrieve all artists

select * from artists;