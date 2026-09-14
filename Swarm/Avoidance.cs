using MissionPlanner.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionPlanner.Swarm
{
    public class Avoidance
    {
        enum MAV_COLLISION_THREAT_LEVEL
        {
            MAV_COLLISION_THREAT_LEVEL_NONE = 0, /* 没有威胁 | */
            MAV_COLLISION_THREAT_LEVEL_LOW = 1, /* 威胁较低 | */
            MAV_COLLISION_THREAT_LEVEL_HIGH = 2, /* 威胁很高 | */
            MAV_COLLISION_THREAT_LEVEL_ENUM_END = 3, /*  | */
        }
        enum MAV_COLLISION_ACTION
        {
            MAV_COLLISION_ACTION_NONE = 0, /* 忽略任何潜在的碰撞 | */
            MAV_COLLISION_ACTION_REPORT = 1, /* 报告潜在的碰撞 | */
            MAV_COLLISION_ACTION_ASCEND_OR_DESCEND = 2, /* 上升或下降以避免碰撞 | */
            MAV_COLLISION_ACTION_MOVE_HORIZONTALLY = 3, /* 水平移动以避免碰撞| */
            MAV_COLLISION_ACTION_MOVE_PERPENDICULAR = 4, /* 飞机垂直于碰撞的速度矢量移动 | */
            MAV_COLLISION_ACTION_ENUM_END = 5, /*  | */
        }
        class Obstacle
        {
            public uint src_id;
            public DateTime timestamp;

            public PointLatLngAlt location = new PointLatLngAlt();
            public Vector3 velocity = new Vector3();  // ned

            public MAV_COLLISION_THREAT_LEVEL threat_level = MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_NONE;
            public float closest_approach_xy; // metres
            public float closest_approach_z; // metres
            public float time_to_closest_approach; // seconds, 3D approach
            public float distance_to_closest_approach; // metres, 3D
        };
        List<Obstacle> Obstacles = new List<Obstacle>();
        int current_most_serious_threat = -1;
        // 避障的相关参数设置
        uint MAX_OBSTACLE_AGE_MS = 5000;
        float _fail_time_horizon = 3;
        float _warn_time_horizon = 3;
        float _fail_distance_xy = 40;
        float _fail_distance_z = 15;
        float _warn_distance_xy = 80;
        float _warn_distance_z = 80;
        float _fail_altitude_minimum = 30; // 最低的高度限制，低于此高度不避障
        MAV_COLLISION_ACTION _fail_action = MAV_COLLISION_ACTION.MAV_COLLISION_ACTION_MOVE_PERPENDICULAR;
        uint _avoidance_vertical_m = 20;
        uint _avoidance_horizontal_m = 50;
        float _min_approach_time_s = 5;

        public void set_obstacles(List<MAVState> mAVStates)
        {
            Obstacles.Clear();
            foreach (var mAVState in mAVStates)
            {
                Obstacle obstacle = new Obstacle();
                obstacle.src_id = mAVState.sysid;
                obstacle.timestamp = mAVState.lastvalidpacket;
                obstacle.location = new PointLatLngAlt(mAVState.cs.lat, mAVState.cs.lng,mAVState.cs.alt);
                obstacle.velocity = new Vector3(mAVState.cs.vx, mAVState.cs.vy, mAVState.cs.vz);
                Obstacles.Add(obstacle);
            }
        }
        public void set_params(FormationControl.FormationOverallParameters overallParameters)
        {
            _fail_time_horizon = overallParameters.FailTimeHorizon;
            _warn_time_horizon = overallParameters.WarnTimeHorizon;
            _fail_distance_xy = overallParameters.FailDistance_xy;
            _fail_distance_z = overallParameters.FailDistance_z;
            _warn_distance_xy = overallParameters.WarnDistance_xy;
            _warn_distance_z = overallParameters.WarnDistance_z;
            _fail_altitude_minimum = overallParameters.FailAltitudeMinimum;
            _fail_action = (MAV_COLLISION_ACTION)overallParameters.FailAction;
            _avoidance_vertical_m = overallParameters.AvoidanceVertical_m;
            _avoidance_horizontal_m = overallParameters.AvoidanceHorizontal_m;
            _min_approach_time_s = overallParameters.MinApproachTime_s;
        }
        public byte get_obstacleid()
        {
            if (current_most_serious_threat < 0)
            {
                return 0;
            }
            if (most_serious_threat()==null)
            {
                return 0;
            }
            return (byte)most_serious_threat().src_id;
        }
        public Vector3 update(MAVState mav)
        {
            check_for_threats(mav);
            return handle_avoidance_local(most_serious_threat(),mav);
        }
        void check_for_threats(MAVState mav)
        {
            if (mav.cs.Location.Lat == 0 || mav.cs.Location.Lng == 0)
            {
                return;
            }
            PointLatLngAlt my_loc = new PointLatLngAlt(mav.cs.lat, mav.cs.lng, mav.cs.alt);
            Vector3 my_vel = new Vector3(mav.cs.vx, mav.cs.vy, mav.cs.vz);
            current_most_serious_threat = -1;
            for (int i = 0; i < Obstacles.Count; i++)
            {
                Obstacle obstacle = Obstacles[i];
                float obstacle_age = (float)DateTime.Now.Subtract(obstacle.timestamp).TotalMilliseconds;
                if (obstacle_age > MAX_OBSTACLE_AGE_MS)
                {
                    continue;
                }
                update_threat_level(my_loc, my_vel, ref obstacle);
                if (obstacle_is_more_serious_threat(obstacle))
                {
                    current_most_serious_threat = i;
                }
            }
        }
        Vector3 handle_avoidance_local(Obstacle threat, MAVState mav)
        {
            MAV_COLLISION_THREAT_LEVEL new_threat_level = MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_NONE;
            MAV_COLLISION_ACTION action = MAV_COLLISION_ACTION.MAV_COLLISION_ACTION_NONE;
            if (threat != null)
            {
                new_threat_level = threat.threat_level;
                if (new_threat_level == MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_HIGH)
                {
                    action = _fail_action;
                    if (action != MAV_COLLISION_ACTION.MAV_COLLISION_ACTION_NONE && _fail_altitude_minimum > 0 &&
                     mav.cs.alt< _fail_altitude_minimum)
                    {
                        // 当飞机距离地面太近，不避障
                        action = MAV_COLLISION_ACTION.MAV_COLLISION_ACTION_REPORT;
                    }
                }
            }
            if ((threat != null) && (new_threat_level == MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_HIGH) && (action > MAV_COLLISION_ACTION.MAV_COLLISION_ACTION_REPORT))
            {
                return handle_avoidance(threat, action, mav);
            }
            else
            {
                return new Vector3(0,0,0);
            }
        }
        Vector3 handle_avoidance(Obstacle obstacle, MAV_COLLISION_ACTION requested_action, MAVState mav)
        {
            Vector3 temp = new Vector3(0, 0, 0);
            MAV_COLLISION_ACTION actual_action = requested_action;
            switch (actual_action)
            {

                case MAV_COLLISION_ACTION.MAV_COLLISION_ACTION_ASCEND_OR_DESCEND:
                    // 爬升或下降以避开障碍物
                    temp.z = handle_avoidance_vertical(obstacle, mav);
                    break;

                case MAV_COLLISION_ACTION.MAV_COLLISION_ACTION_MOVE_HORIZONTALLY:
                    // 水平移动以避开障碍物
                    handle_avoidance_horizontal(obstacle, mav,out temp.x, out temp.y);
                    break;

                case MAV_COLLISION_ACTION.MAV_COLLISION_ACTION_MOVE_PERPENDICULAR:
                    {
                        // 水平和垂直移动以避开障碍物
                        temp.z = handle_avoidance_vertical(obstacle, mav);
                        handle_avoidance_horizontal(obstacle, mav, out temp.x, out temp.y);
                    }
                    break;
                case MAV_COLLISION_ACTION.MAV_COLLISION_ACTION_NONE:
                    break;
                case MAV_COLLISION_ACTION.MAV_COLLISION_ACTION_REPORT:
                    break;
                default:
                    break;
            }
            return temp;
        }
        double handle_avoidance_vertical(Obstacle obstacle,MAVState mav)
        {
            // 让高的更高，低的更低
            if (mav.cs.alt > obstacle.location.Alt)
            {
                return mav.cs.alt + _avoidance_vertical_m;
            }
            else
            {
                return mav.cs.alt - _avoidance_vertical_m;
            }
        }
        void handle_avoidance_horizontal(Obstacle obstacle, MAVState mav,out double x,out double y)
        {
            // 从障碍物中获得最佳矢量
            Vector3 velocity_neu = new Vector3();
            if (get_vector_perpendicular(obstacle, mav,out velocity_neu))
            {
                // 移除垂向的影响
                velocity_neu.z = 0.0f;
                // re-normalize
                velocity_neu.normalize();

                // 移动100m
                velocity_neu *= _avoidance_horizontal_m;
                x = velocity_neu.x;
                y = velocity_neu.y;
            }
            else
            {
                x = y = 0;
            }
        }
        bool get_vector_perpendicular(Obstacle obstacle, MAVState mav, out Vector3 vec_neu)
        {
            if (obstacle == null)
            {
                vec_neu = new Vector3();
                return false;
            }
            if (mav.cs.lat == 0|| mav.cs.lng==0)
            {
                vec_neu = new Vector3();
                return false;
            }
            if (obstacle.velocity.length() < 1)
            {
                System.Numerics.Vector2 delta_pos_xy = location_diff(obstacle.location, mav.cs.Location);
                double delta_pos_z = mav.cs.alt - obstacle.location.Alt;
                Vector3 delta_pos_xyz =new Vector3(delta_pos_xy.X, delta_pos_xy.Y, delta_pos_z);
                // 防止除0
                if (delta_pos_xyz.length().Equals(0))
                {
                    vec_neu = new Vector3();
                    return false;
                }
                delta_pos_xyz.normalize();
                vec_neu = delta_pos_xyz;
                return true;
            }
            else
            {
                vec_neu = perpendicular_xyz(obstacle.location, obstacle.velocity, new PointLatLngAlt(mav.cs.lat,mav.cs.lng,mav.cs.alt));
                // 防止除0
                if (vec_neu.length().Equals(0))
                {
                    return false;
                }
                vec_neu.normalize();
                return true;
            }
        }
        Vector3 perpendicular_xyz(PointLatLngAlt p1, Vector3 v1, PointLatLngAlt p2)
        {
            System.Numerics.Vector2 delta_p_2d = location_diff(p1, p2);
            Vector3 delta_p_xyz = new Vector3(delta_p_2d.X, delta_p_2d.Y, (p2.Alt - p1.Alt)); //check this line
            Vector3 v1_xyz = new Vector3(v1[0], v1[1], -v1[2]);
            Vector3 ret =perpendicular(delta_p_xyz, v1_xyz);
            return ret;
        }
        Vector3 perpendicular(Vector3 p1, Vector3 v1)
        {
            float d = (float)(p1 * v1);
            if (Math.Abs(d) < FLT_EPSILON)
            {
                return p1;
            }
            Vector3 parallel =new Vector3((v1 * d) / (v1.length()* v1.length()));
            Vector3 perpendicular = new Vector3(p1 - parallel);

            return perpendicular;
        }
        void update_threat_level(PointLatLngAlt my_loc, Vector3 my_vel, ref Obstacle obstacle)
        {
            obstacle.threat_level = MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_NONE;

            float obstacle_age = (float)DateTime.Now.Subtract(obstacle.timestamp).TotalMilliseconds;
            float closest_xy = closest_approach_xy(my_loc, my_vel, obstacle.location, obstacle.velocity, _fail_time_horizon + obstacle_age / 1000);
            // 进入避障的区域
            if (closest_xy < _fail_distance_xy)
            {
                obstacle.threat_level = MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_HIGH;
            }
            else
            {
                closest_xy = closest_approach_xy(my_loc, my_vel, obstacle.location, obstacle.velocity, _warn_time_horizon + obstacle_age / 1000);
                if (closest_xy < _warn_distance_xy)
                {
                    obstacle.threat_level = MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_LOW;
                }
            }

            // 检查垂向
            // 如果垂向有相差，可以将威胁等级降低
            float closest_z = closest_approach_z(my_loc, my_vel, obstacle.location, obstacle.velocity, _warn_time_horizon + obstacle_age / 1000);
            if (obstacle.threat_level != MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_NONE)
            {
                if (closest_z > _warn_distance_z)
                {
                    obstacle.threat_level = MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_NONE;
                }
                else
                {
                    closest_z = closest_approach_z(my_loc, my_vel, obstacle.location, obstacle.velocity, _fail_time_horizon + obstacle_age / 1000);
                    if (closest_z > _fail_distance_z)
                    {
                        obstacle.threat_level = MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_LOW;
                    }
                }
            }

            // 若障碍物（其他飞机）通信断开，将其危险等级取消
            if (obstacle_age > MAX_OBSTACLE_AGE_MS)
            {
                obstacle.threat_level = MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_NONE;
            }
            obstacle.closest_approach_xy = closest_xy;
            obstacle.closest_approach_z = closest_z;
            // 计算间距
            float current_distance = (float)my_loc.GetDistance(obstacle.location);
            obstacle.distance_to_closest_approach = current_distance - closest_xy;
            // 相对速度ne向量
            System.Numerics.Vector2 net_velocity_ne = new System.Numerics.Vector2((float)(my_vel[0] - obstacle.velocity[0]), (float)(my_vel[1] - obstacle.velocity[1]));
            // 计算相对速度向量长度
            float net_velocity_ne_length = net_velocity_ne.Length();
            obstacle.time_to_closest_approach = 0.0f;
            if (obstacle.distance_to_closest_approach!=0&& net_velocity_ne_length!=0)
            {
                obstacle.time_to_closest_approach = obstacle.distance_to_closest_approach / net_velocity_ne_length;
            }
            //float distance_to_closest_approach = 0;
            //float time_to_closest_approach = 0;
            //// 相对距离ne向量
            //System.Numerics.Vector2 net_position_ne = location_diff(my_loc, obstacle.location);
            //// 计算在相对距离在相对速度方向上的投影（向量内积）

            //distance_to_closest_approach = Math.Abs(net_position_ne.X * System.Numerics.Vector2.Normalize(net_velocity_ne).X + net_position_ne.Y * System.Numerics.Vector2.Normalize(net_velocity_ne).Y);
            
            //// 如果相对速度等于0，或者相对速度向量和相对距离向量反向(内积小于0)，则认为接近时间无穷大
            //if (net_velocity_ne_length < FLT_EPSILON || (net_position_ne.X * net_velocity_ne.X + net_position_ne.X * net_velocity_ne.X) < 0)
            //{
            //    time_to_closest_approach = float.MaxValue;
            //}
            //else
            //{
            //    time_to_closest_approach = obstacle.distance_to_closest_approach / net_velocity_ne_length;
            //}
            //// 解除威胁条件：两飞机有一定的间距(考虑传感器误差)，预计碰撞时间大于设定的最小接近时间
            //if (obstacle.threat_level != MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_NONE && current_distance > 5 && time_to_closest_approach > _min_approach_time_s)
            //{
            //    obstacle.threat_level = MAV_COLLISION_THREAT_LEVEL.MAV_COLLISION_THREAT_LEVEL_NONE;
            //}
        }
        Obstacle most_serious_threat()
        {
            if (current_most_serious_threat < 0)
            {
                // we *really_ should not have been called!
                return null;
            }
            return Obstacles[current_most_serious_threat];
        }

        float closest_approach_xy(PointLatLngAlt my_loc, Vector3 my_vel, PointLatLngAlt obstacle_loc, Vector3 obstacle_vel, float time_horizon)
        {
            System.Numerics.Vector2 delta_vel_ne = new System.Numerics.Vector2((float)(obstacle_vel[0] - my_vel[0]), (float)(obstacle_vel[1] - my_vel[1]));
            System.Numerics.Vector2 delta_pos_ne = location_diff(obstacle_loc, my_loc);
            System.Numerics.Vector2 line_segment_ne = new System.Numerics.Vector2();
            line_segment_ne = delta_vel_ne * time_horizon;
            float ret = closest_distance_between_radial_and_point(line_segment_ne,delta_pos_ne);
            return ret;
        }

        static float LOCATION_SCALING_FACTOR = 0.011131884502145034f;

        static float LOCATION_SCALING_FACTOR_INV = 89.83204953368922f;
        public static System.Numerics.Vector2 location_diff(PointLatLngAlt loc1, PointLatLngAlt loc2)
        {
            return new System.Numerics.Vector2((float)((loc2.Lat - loc1.Lat) *1e7* LOCATION_SCALING_FACTOR),
                                               (float)((loc2.Lng - loc1.Lng) * 1e7 * LOCATION_SCALING_FACTOR* longitude_scale(loc1)));
        }
        public static float cross_mul(System.Numerics.Vector2 A, System.Numerics.Vector2 B)
        {
            return A.X * B.Y - A.Y * B.X;
        }
        public static float dot_mul(System.Numerics.Vector2 A, System.Numerics.Vector2 B)
        {
            return A.X * B.X + A.Y * B.Y;
        }
        public static float longitude_scale(PointLatLngAlt loc)
        {
            double scale = Math.Cos(loc.Lat* MathHelper.deg2rad);
            return (float)MathHelper.constrain(scale, 0.01, 1.0);
        }
        float closest_distance_between_radial_and_point(System.Numerics.Vector2 w, System.Numerics.Vector2 p)
        {
            System.Numerics.Vector2 closest = new System.Numerics.Vector2();
            System.Numerics.Vector2 delta = new System.Numerics.Vector2();
            closest = closest_point(p, new System.Numerics.Vector2(0, 0), w);
            delta = closest - p;
            return delta.Length();
        }
        float FLT_EPSILON = 1.1920929e-07F;
        System.Numerics.Vector2 closest_point(System.Numerics.Vector2 p, System.Numerics.Vector2 v, System.Numerics.Vector2 w)
        {
            // length squared of line segment
            float l2 = (v - w).LengthSquared();
            if (l2 < FLT_EPSILON)
            {
                // v == w case
                return v;
            }
            // Consider the line extending the segment, parameterized as v + t (w - v).
            // We find projection of point p onto the line.
            // It falls where t = [(p-v) . (w-v)] / |w-v|^2
            // We clamp t from [0,1] to handle points outside the segment vw.
            float t = ((p - v).X * (w - v).X + (p - v).Y * (w - v).Y) / l2;
            if (t <= 0)
            {
                return v;
            }
            else if (t >= 1)
            {
                return w;
            }
            else
            {
                return v + (w - v) * t;
            }
        }
        float closest_approach_z(PointLatLngAlt my_loc, Vector3 my_vel, PointLatLngAlt obstacle_loc, Vector3 obstacle_vel, float time_horizon)
        {
            float delta_vel_d = -(float)(obstacle_vel[2] - my_vel[2]);
            float delta_pos_d = (float)(obstacle_loc.Alt - my_loc.Alt);

            float ret;
            // 障碍物高，且障碍物速度小于我方速度，由于速度NED，越小意味这障碍物越倾向于向上运动
            if (delta_pos_d >= 0 && delta_vel_d >= 0)
            {
                ret = delta_pos_d;
            }/*障碍物低，且障碍物速度大于我方速度，由于速度NED，越大意味这障碍物越倾向于向下运动*/
            else if (delta_pos_d <= 0 && delta_vel_d <= 0)
            {
                ret = Math.Abs(delta_pos_d);
            }
            else
            {
                ret = Math.Abs(delta_pos_d - delta_vel_d * time_horizon);
            }
            return ret;
        }
        bool obstacle_is_more_serious_threat(Obstacle obstacle)
        {
            if (current_most_serious_threat == -1)
            {
                // any threat is more of a threat than no threat
                return true;
            }
            Obstacle current = Obstacles[current_most_serious_threat];
            if (obstacle.threat_level > current.threat_level)
            {
                // threat_level is updated by update_threat_level
                return true;
            }
            if (obstacle.threat_level == current.threat_level &&
                obstacle.time_to_closest_approach < current.time_to_closest_approach)
            {
                return true;
            }
            return false;
        }
    }
}
