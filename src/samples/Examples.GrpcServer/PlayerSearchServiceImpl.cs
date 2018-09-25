using Examples.GrpcModels;
using Grpc.Core;
using Grpc.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Examples.GrpcServer
{
    public class PlayerSearchServiceImpl : GrpcModels.PlayerSearch.PlayerSearchBase
    {
        private static readonly int s_FetchSize = 2;
        private static readonly int s_DelayMilliseconds = 200;
        private static readonly ILogger logger = GrpcEnvironment.Logger.ForType<PlayerSearchServiceImpl>();

        #region Unary method examle

        /// <summary>
        /// SearchTeam
        /// </summary>
        public override Task<TeamSearchResponse> SearchTeam(TeamSearchRequest request, ServerCallContext context)
        {
            logger.Info(string.Format("[{0}] Requested {1} players.", request.Name, request.ExpectedDataCount));
            TeamSearchResponse response = new TeamSearchResponse();
            response.Teams.AddRange(new List<Team> {
                new Team{
                    Code="c1",
                    Country="Co1",
                    Name="n1"
                },
                new Team{
                    Code = "c2",
                    Country = "Co2",
                    Name = request.Name
                }
            });
            return Task.FromResult(response);
        }

        #endregion Unary method examle

        #region ServerStream method examle

        /// <summary>
        /// SearchPlayer_ServerStream
        /// </summary>
        public async override Task SearchPlayer_ServerStream(PlayerSearchRequest request, IServerStreamWriter<PlayerSearchResponse> responseStream, ServerCallContext context)
        {
            logger.Info(string.Format("[{0}] Requested {1} players.", request.PlayerName, request.ExpectedDataCount));
            PlayerSearchResponse response = new PlayerSearchResponse();

            int fetchCount = 0;
            var Players = new List<Player>() { new Player {
                Age=1,
                Name="11",
                TeamCode="11",
            },
            new Player {
                Age=2,
                Name="22",
                TeamCode="22",
            },
            new Player {
                Age=3,
                Name="33",
                TeamCode="33",
            }};
            foreach (Player player in Players)
            {
                response.Players.Add(player);

                ++fetchCount;

                if (fetchCount == s_FetchSize)
                {
                    await Task.Delay(s_DelayMilliseconds).ConfigureAwait(false);
                    await responseStream.WriteAsync(response).ConfigureAwait(false);
                    response.Players.Clear();
                    response.Teams.Clear();
                    fetchCount = 0;
                }
            }

            if (response.Players.Count > 0)
            {
                await Task.Delay(s_DelayMilliseconds).ConfigureAwait(false);
                await responseStream.WriteAsync(response).ConfigureAwait(false);
            }
        }

        #endregion ServerStream method examle

        #region ClientStream method examle

        /// <summary>
        /// SearchPlayer_ClientStream
        /// </summary>
        public async override Task<PlayerSearchResponse> SearchPlayer_ClientStream(IAsyncStreamReader<PlayerSearchRequest> requestStream, ServerCallContext context)
        {
            PlayerSearchResponse response = new PlayerSearchResponse();

            int initial = 1;

            while (await requestStream.MoveNext().ConfigureAwait(false))
            {
                PlayerSearchRequest request = requestStream.Current;
                logger.Info(string.Format("[{0}] Requested {1} players.", "", request.ExpectedDataCount));
                var Players = new List<Player>() { new Player {
                    Age=1,
                    Name="11",
                    TeamCode="11",
                },
                new Player {
                    Age=2,
                    Name="22",
                    TeamCode="22",
                },
                new Player {
                    Age=3,
                    Name="33",
                    TeamCode="33",
                }};
                foreach (Player player in Players)
                {
                    response.Players.Add(player);

                    if (!response.Teams.ContainsKey(player.TeamCode))
                    {
                        response.Teams.Add(player.TeamCode, new Team
                        {
                            Code = player.TeamCode,
                            Country = player.TeamCode,
                            Name = player.TeamCode
                        });
                    }
                }

                initial += request.ExpectedDataCount;
            }

            return response;
        }

        #endregion ClientStream method examle

        #region DuplexStream method examle

        /// <summary>
        /// SearchPlayer_DuplexStream
        /// </summary>
        public async override Task SearchPlayer_DuplexStream(IAsyncStreamReader<PlayerSearchRequest> requestStream, IServerStreamWriter<PlayerSearchResponse> responseStream, ServerCallContext context)
        {
            int initial = 1;

            while (await requestStream.MoveNext().ConfigureAwait(false))
            {
                PlayerSearchResponse response = new PlayerSearchResponse();

                PlayerSearchRequest request = requestStream.Current;

                logger.Info(string.Format("[{0}] Requested {1} players.", "", request.ExpectedDataCount));

                int fetchCount = 0;
                var Players = new List<Player>() { new Player {
                    Age=1,
                    Name="11",
                    TeamCode="11",
                },
                new Player {
                    Age=2,
                    Name="22",
                    TeamCode="22",
                },
                new Player {
                    Age=3,
                    Name="33",
                    TeamCode="33",
                }};
                foreach (Player player in Players)
                {
                    response.Players.Add(player);

                    if (!response.Teams.ContainsKey(player.TeamCode))
                    {
                        response.Teams.Add(player.TeamCode, new Team
                        {
                            Code = player.TeamCode,
                            Country = player.TeamCode,
                            Name = player.TeamCode
                        });
                    }

                    ++fetchCount;

                    if (fetchCount == s_FetchSize)
                    {
                        await Task.Delay(s_DelayMilliseconds).ConfigureAwait(false);
                        await responseStream.WriteAsync(response).ConfigureAwait(false);

                        response.Players.Clear();
                        response.Teams.Clear();
                        fetchCount = 0;
                    }
                }

                if (response.Players.Count > 0)
                {
                    await Task.Delay(s_DelayMilliseconds).ConfigureAwait(false);
                    await responseStream.WriteAsync(response).ConfigureAwait(false);
                }

                initial += request.ExpectedDataCount;
            }
        }

        #endregion DuplexStream method examle
    }
}