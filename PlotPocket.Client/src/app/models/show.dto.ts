export enum ShowType {
  Movie = 'Movie',
  TvShow = 'TvShow',
}

export interface ShowDto {
  id: number;
  title: string;
  overview: string;
  PosterPath?: string;
  posterPath?: string;
  rating: number;
  ReleaseDate?: Date;
  type: ShowType;
  isWatchlisted: boolean;
  isInWatchlist: boolean;
  isWatched: boolean;
  isBookmarked: boolean;
}
